#pragma warning disable IDE0040

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;

using DSharpPlus.Voice.MemoryServices;
using DSharpPlus.Voice.MemoryServices.Channels;
using DSharpPlus.Voice.Protocol.RTCP;
using DSharpPlus.Voice.Protocol.RTCP.Payloads;
using DSharpPlus.Voice.Protocol.RTP;

using Microsoft.Extensions.Logging;

using static DSharpPlus.Voice.Protocol.RTCP.RTCPIntervalHelper;

namespace DSharpPlus.Voice;

partial class VoiceConnection
{
    private async Task ReceiveAudioAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using ArrayPoolBufferWriter<byte> receiveWriter = new();
            using ArrayPoolBufferWriter<byte> decryptedWriter = new();

            await this.mediaTransport.ReceiveAsync(receiveWriter);

            ulong userId;
            VoiceUser? voiceUser;

            // if it's 8 bytes it's the UDP heartbeat/keepalive, or potentially a RTCP Goodbye packet
            if (receiveWriter.WrittenCount == 8 && !RTCPSerializer.IsValidRTCPPacket(receiveWriter.WrittenSpan))
            {
                this.metrics.RecordKeepaliveReceived();
                long timestamp = BinaryPrimitives.ReadInt64BigEndian(receiveWriter.WrittenSpan);
                TimeSpan delay = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(timestamp);

                this.AudioConnectionLatency = delay;

                if (delay > TimeSpan.FromSeconds(5))
                {
                    this.logger.LogWarning("The latency of the audio connection exceeds five seconds.");
                }

                continue;
            }
            else if (RTCPSerializer.IsValidRTCPPacket(receiveWriter.WrittenSpan))
            {
                UpdateAverageRTCPPacketSize(receiveWriter.WrittenCount);

                IReadOnlyList<IRTCPPacket> rtcpPackets = RTCPSerializer.Deserialize(receiveWriter.WrittenSpan);

                this.metrics.RecordControlPacketReceived(receiveWriter.WrittenCount, rtcpPackets.Count);
                
                foreach (IRTCPPacket singlePacket in rtcpPackets)
                {
                    if (this.logger.IsEnabled(LogLevel.Trace))
                    {
                        this.logger.LogTrace("Received RTCP packet {packet}", singlePacket);
                    }

                    if (singlePacket is RTCPSenderReportPacket senderReportPacket)
                    {
                        if (!this.ssrcs.TryGetValue(senderReportPacket.SSRC, out userId))
                        {
                            // race condition: we're still waiting on their SSRC, but they're already sending RTCP reports, just ignore
                            continue;
                        }

                        if (!this.voiceUsers.TryGetValue(userId, out voiceUser))
                        {
                            // something insane has happened. this is probably? impossible
                            throw new UnreachableException();
                        }

                        voiceUser.LastSenderReport = DateTimeOffset.UtcNow;
                    }
                }

                continue;
            }

            this.metrics.RecordAudioFrameReceived(receiveWriter.WrittenCount);

            this.cryptor.Decrypt(receiveWriter.WrittenSpan, decryptedWriter, out RTPFrameInfo frameInfo);
            
            if (!this.ssrcs.TryGetValue(frameInfo.SSRC, out userId))
            {
                // race condition: we're still waiting on their ssrc but they already started speaking
                // [TODO] consider buffering such received packets?
                continue;
            }

            if (!this.voiceUsers.TryGetValue(userId, out voiceUser))
            {
                // something insane has happened. this is probably? impossible
                throw new UnreachableException();
            }

            (uint normalizedSequence, ulong normalizedTimestamp) = voiceUser.UpdateTimestampAndSequence(frameInfo.Sequence, frameInfo.Timestamp);

            // handle rtp padding
            int audioBytes = decryptedWriter.WrittenCount;

            if (BitHelper.HasFlag(receiveWriter.WrittenSpan[0], 3))
            {
                int paddingBytes = decryptedWriter.WrittenSpan[^1];
                audioBytes -= paddingBytes;

                if (audioBytes == 0)
                {
                    this.metrics.RecordEmptyAudioFrameReceived();
                    continue;
                }
            }

            // the transport cryptor already is supposed to only decrypt the audio data (the header counts as additional data)
            byte[] audio = ArrayPool<byte>.Shared.Rent(audioBytes);
            int finalDecryptedLength = this.e2ee.DecryptFrame(userId, decryptedWriter.WrittenSpan, audio);

            // native returns 0 if it failed
            if (finalDecryptedLength == 0)
            {
                this.metrics.RecordAudioFrameFailedDecryption();
                continue;
            }

            TimeSpan duration = this.codec.CalculatePacketLength(audio);

            await this.Receiver.ProcessAudioAsync(frameInfo.SSRC, normalizedSequence, new(normalizedTimestamp), duration, audio[..finalDecryptedLength]);

            ArrayPool<byte>.Shared.Return(audio);
        }
    }

    private async Task AudioKeepaliveAsync(CancellationToken ct)
    {
        PeriodicTimer timer = new(TimeSpan.FromMilliseconds(5000));
        byte[] buffer = new byte[8];
        
        while (await timer.WaitForNextTickAsync(ct))
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            BinaryPrimitives.WriteInt64BigEndian(buffer, currentTime);

            await this.mediaTransport.SendAsync(buffer);
            this.metrics.RecordKeepaliveSent();
        }
    }

    private async Task SendAudioAsync(CancellationToken ct)
    {
        int counter = 1;
        int silenceCounter = 0;
        bool framePrepared = true;
        // no real point pooling this, since we use it for the entire lifetime
        byte[] currentFrame = new byte[this.e2ee.GetMaxEncryptedLength(5760) + 12]; 
        ushort sequence = (ushort)Random.Shared.NextInt64();
        PeriodicTimer timer = new(TimeSpan.FromMilliseconds(20));

        AudioBufferLease lease;
        int length;

        while (!ct.IsCancellationRequested)
        {
            if (this.selfMute)
            {
                await this.noLongerSelfMuted.Task;
            }

            await this.sendingAudioChannel.Reader.WaitToReadAsync(ct);

            AudioChannelReadResult readResult = await this.sendingAudioChannel.Reader.ReadAsync(ct);

            if (readResult.State == AudioChannelState.Terminated)
            {
                break;
            }

            this.timestamp.RealignTimestamp();

            lease = readResult.Buffer.Value;
            length = WriteAndEncryptFrame(lease.Buffer, currentFrame, this.timestamp.Value, sequence);
            lease.Dispose();

            // we have audio available, start sending it
            while (await timer.WaitForNextTickAsync(ct))
            {
                this.timestamp.Add(20);

                // we were processing a long frame and haven't yet waited long enough, just wait until the next tick
                if (--counter > 0)
                {
                    continue;
                }

                // this must be incremented after the early return, otherwise we produce wrong sequences here
                _ = unchecked(sequence++);

                if (framePrepared)
                {
                    if (!this.IsSpeaking)
                    {
                        this.logger.LogTrace("Commencing audio transmission to the remote server.");
                        await StartSpeakingAsync();
                    }

                    silenceCounter = 0;
                    this.packetsSent++;
                    this.opusBytesSent += (uint)length - 12;

                    await this.mediaTransport.SendAsync(currentFrame.AsMemory()[..length]);

                    this.metrics.RecordAudioFrameSent(length);
                }
                else
                {
                    silenceCounter++;
                }

                // if the silence counter has exceeded the configured maximum, fall silent and pause
                if (silenceCounter >= this.options.SilenceTicksBeforePausingConnection)
                {
                    await SendFiveSilenceFramesAsync();
                    break;
                }

                // otherwise, let's go and encrypt the next bit of audio
                readResult = this.sendingAudioChannel.Reader.TryRead();

                if (!readResult.IsAvailable)
                {
                    framePrepared = false;

                    // if we're told to not expect any audio soon, just shut up
                    if (readResult.State is AudioChannelState.Paused or AudioChannelState.Terminated)
                    {
                        await SendFiveSilenceFramesAsync();
                        break;
                    }
                    
                    continue;
                }

                lease = readResult.Buffer.Value;

                counter = lease.FrameCount;
                length = WriteAndEncryptFrame(lease.Buffer, currentFrame, this.timestamp.Value, sequence);
                lease.Dispose();
            }
        }

        async Task SendFiveSilenceFramesAsync()
        {
            this.logger.LogTrace("Pausing audio transmission to the remote server.");
            await StopSpeakingAsync();

            for (int i = 0; i < 5; i++)
            {
                lease = this.encoder.WriteSilenceFrame();
                length = WriteAndEncryptFrame(lease.Buffer, currentFrame, this.timestamp.Value, sequence);
                lease.Dispose();

                this.packetsSent++;
                this.opusBytesSent += 3;
                await this.mediaTransport.SendAsync(currentFrame.AsMemory()[..length]);

                this.metrics.RecordAudioFrameSent(length);
            }
        }
    }

    private async Task SendRTCPReportsAsync(CancellationToken ct)
    {
        RTCPSourceDescriptionPacket description = new()
        {
            SourceDescriptions = [new()
            {
                SSRC = this.ssrc,
                DescriptionItems = 
                [
                    new()
                    {
                        Type = SourceDescriptionItemType.CanonicalName,
                        Value = this.userId.ToString()
                    },
                    new()
                    {
                        Type = SourceDescriptionItemType.ApplicationName,
                        Value = $"DSharpPlus.Voice v{Utilities.Version}"
                    }
                ]
            }]
        };

        TimeSpan initialDelay = CalculateInitialRTCPInterval(this.connectedUsers.Count, this.bitrate);

        await Task.Delay((int)initialDelay.TotalMilliseconds, ct);

        IRTCPPacket report = CollectAndWriteRTCPReport();
        await SendRTCPPacketsAsync([report, description]);

        // recalculate the delay after initial calibration
        TimeSpan afterRecalibration = CalculateRTCPInterval
        (
            this.connectedUsers.Count,
            this.voiceUsers.Count(x => x.Value.IsSpeaking),
            this.bitrate,
            this.averageRTCPPacketSize,
            this.IsSpeaking
        );

        this.rtcpReportTimer.Period = afterRecalibration;

        while(await this.rtcpReportTimer.WaitForNextTickAsync(ct))
        {
            report = CollectAndWriteRTCPReport();
            await SendRTCPPacketsAsync([report, description]);
        }
    }

    private async Task SendRTCPPacketsAsync(IReadOnlyList<IRTCPPacket> packets)
    {
        if (this.logger.IsEnabled(LogLevel.Trace) && this.options.LogOutboundRTCPPackets)
        {
            foreach (IRTCPPacket packet in packets)
            {
                this.logger.LogTrace("Sending RTCP packet: {packet}", packet);
            }
        }

        ArrayPoolBufferWriter<byte> writer = new();

        foreach (IRTCPPacket packet in packets)
        {
            RTCPSerializer.Serialize(packet, writer);
        }

        UpdateAverageRTCPPacketSize(writer.WrittenCount);

        await this.mediaTransport.SendAsync(writer.WrittenMemory);

        this.metrics.RecordControlPacketSent(writer.WrittenCount, packets.Count);
    }

    private IRTCPPacket CollectAndWriteRTCPReport()
    {
        List<ReceptionReport> receptionReports = [];

        foreach (VoiceUser user in this.voiceUsers.Values)
        {
            if (!user.IsSpeaking)
            {
                continue;
            }

            ReceptionReport report = new()
            {
                SSRC = user.SSRC,
                HighestSequenceReceived = user.HighestSequenceReceived,
                InterarrivalJitter = (uint)user.InterarrivalJitterEstimate,
                PacketLoss = user.PacketLoss,
                CumulativePacketsLost = user.CumulativePacketsLost,
                LastSenderReportTimestamp = user.LastSenderReport,
                DelaySinceLastSenderReport = DateTimeOffset.UtcNow - user.LastSenderReport
            };

            receptionReports.Add(report);
        }

        if (this.IsSpeaking)
        {
            return new RTCPSenderReportPacket
            {
                SSRC = this.ssrc,
                Timestamp = DateTimeOffset.UtcNow,
                RTPTimestamp = this.timestamp.ExactValue,
                PacketsSent = this.packetsSent,
                BytesSent = this.opusBytesSent,
                ReceptionReports = receptionReports
            };
        }
        else
        {
            return new RTCPReceiverReportPacket
            {
                SSRC = this.ssrc,
                ReceptionReports = receptionReports
            };
        }
    }
    
    private int WriteAndEncryptFrame(ReadOnlySpan<byte> unencrypted, Span<byte> target, uint timestamp, ushort sequence)
    {
        using ArrayPoolBufferWriter<byte> e2eeWriter = new();

        RTPHelper.WriteRTPHeader(e2eeWriter.GetSpan(12), sequence, timestamp, this.ssrc);
        e2eeWriter.Advance(12);

        this.e2ee.EncryptFrame(unencrypted, e2eeWriter);
        
        using ArrayPoolBufferWriter<byte> encryptedWriter = new();

        this.cryptor.Encrypt(e2eeWriter.WrittenSpan, encryptedWriter);

        encryptedWriter.WrittenSpan.CopyTo(target);
        return encryptedWriter.WrittenCount;
    }

    private void UpdateAverageRTCPPacketSize(int packetSize)
    {
        // we use a slightly different update formula from RFC 3550 Appendix 7: while they weigh the average to primarily
        // account for the last sixteen or so sent packets, we're going to do the amount of members times two (so their
        // last two reports) plus five to ten percent for assorted nonsense getting sent

        int averagingFactor = (int)(this.connectedUsers.Count * 2 * 1.075);
        this.averageRTCPPacketSize = (this.averageRTCPPacketSize * ((averagingFactor - 1) / averagingFactor))
            + (packetSize / averagingFactor);
    }
}
