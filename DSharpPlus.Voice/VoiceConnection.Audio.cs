#pragma warning disable IDE0040

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.MemoryServices;
using DSharpPlus.Voice.MemoryServices.Channels;
using DSharpPlus.Voice.Protocol.RTCP;
using DSharpPlus.Voice.Protocol.RTCP.Payloads;
using DSharpPlus.Voice.Protocol.RTP;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice;

partial class VoiceConnection
{
    private async Task ReceiveAudioAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            ArrayPoolBufferWriter<byte> receiveWriter = new();
            ArrayPoolBufferWriter<byte> decryptedWriter = new();

            await this.mediaTransport.ReceiveAsync(receiveWriter);

            // if it's 8 bytes it's the UDP heartbeat/keepalive, or potentially a RTCP Goodbye packet
            if (receiveWriter.WrittenCount == 8 && !RTCPSerializer.IsValidRTCPPacket(receiveWriter.WrittenSpan))
            {
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
                IReadOnlyList<IRTCPPacket> rtcpPackets = RTCPSerializer.Deserialize(receiveWriter.WrittenSpan);
                
                if (this.logger.IsEnabled(LogLevel.Trace))
                {
                    foreach (IRTCPPacket singlePacket in rtcpPackets)
                    {
                        this.logger.LogTrace("Received RTCP packet {packet}", singlePacket);
                    }
                }

                // [TODO] handle

                continue;
            }

            this.cryptor.Decrypt(receiveWriter.WrittenSpan, decryptedWriter, out RTPFrameInfo frameInfo);
            
            if (!this.ssrcs.TryGetValue(frameInfo.SSRC, out ulong userId))
            {
                // race condition: we're still waiting on their ssrc but they already started speaking
                // [TODO] consider buffering such received packets?
                continue;
            }

            if (!this.voiceUsers.TryGetValue(userId, out VoiceUser? voiceUser))
            {
                // something insane has happened. this is probably? impossible
                throw new UnreachableException();
            }

            // the transport cryptor already is supposed to only decrypt the audio data (the header counts as additional data)
            byte[] audio = new byte[decryptedWriter.WrittenCount];
            this.e2ee.DecryptFrame(userId, decryptedWriter.WrittenSpan, audio);

            TimeSpan duration = this.codec.CalculatePacketLength(audio);
            (uint normalizedSequence, ulong normalizedTimestamp) = voiceUser.UpdateTimestampAndSequence(frameInfo.Sequence, frameInfo.Timestamp);

            await this.Receiver.ProcessAudioAsync(frameInfo.SSRC, normalizedSequence, new(normalizedTimestamp), duration, audio);
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
        }
    }

    private async Task SendAudioAsync(CancellationToken ct)
    {
        int counter = 1;
        int silenceCounter = 0;
        bool framePrepared = true;
        byte[] currentFrame = new byte[5772]; // no real point pooling this, since we use it for the entire lifetime
        ushort sequence = (ushort)Random.Shared.NextInt64();
        uint timestamp = (uint)Random.Shared.NextInt64();
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

            lease = readResult.Buffer.Value;
            length = WriteAndEncryptFrame(lease.Buffer, currentFrame, timestamp, sequence);
            lease.Dispose();

            // we have audio available, start sending it
            while (await timer.WaitForNextTickAsync(ct))
            {
                unchecked { timestamp += 20; }

                // we were processing a long frame and haven't yet waited long enough, just wait until the next tick
                if (--counter > 0)
                {
                    continue;
                }

                // this must be incremented after the early return, otherwise we produce wrong sequences here
                unchecked { sequence++; }

                if (framePrepared)
                {
                    if (!this.IsSpeaking)
                    {
                        this.logger.LogTrace("Commencing audio transmission to the remote server.");
                        await StartSpeakingAsync();
                    }

                    silenceCounter = 0;
                    await this.mediaTransport.SendAsync(currentFrame.AsMemory()[..length]);
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
                length = WriteAndEncryptFrame(lease.Buffer, currentFrame, timestamp, sequence);
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
                length = WriteAndEncryptFrame(lease.Buffer, currentFrame, timestamp, sequence);
                lease.Dispose();

                await this.mediaTransport.SendAsync(currentFrame.AsMemory()[..length]);
            }
        }
    }

    private async Task SendRTCPPacketsAsync(IReadOnlyList<IRTCPPacket> packets)
    {
        if (this.logger.IsEnabled(LogLevel.Trace))
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

        await this.mediaTransport.SendAsync(writer.WrittenMemory);
    }
    
    private int WriteAndEncryptFrame(ReadOnlySpan<byte> unencrypted, Span<byte> target, uint timestamp, ushort sequence)
    {
        using ArrayPoolBufferWriter<byte> e2eeWriter = new();
        using ArrayPoolBufferWriter<byte> encryptedWriter = new();

        this.e2ee.EncryptFrame(unencrypted, e2eeWriter);

        RTPHelper.WriteRTPHeader(encryptedWriter.GetSpan(12), sequence, timestamp, this.ssrc);
        encryptedWriter.Advance(12);

        this.cryptor.Encrypt(e2eeWriter.WrittenSpan, encryptedWriter);

        encryptedWriter.WrittenSpan.CopyTo(target);
        return encryptedWriter.WrittenCount;
    }
}
