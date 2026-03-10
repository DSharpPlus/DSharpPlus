#pragma warning disable IDE0040

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.MemoryServices;

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

                continue;
            }

            this.cryptor.Decrypt(receiveWriter.WrittenSpan, decryptedWriter, out RTPFrameInfo frameInfo);
            
            if (!this.Receiver.Ssrcs.TryGetValue(frameInfo.SSRC, out VoiceUser? voiceUser))
            {
                // race condition: we're still waiting on their ssrc but they already started speaking
                // [TODO] consider buffering such received packets?
                continue;
            }

            // the transport cryptor already is supposed to only decrypt the audio data (the header counts as additional data)
            byte[] audio = new byte[decryptedWriter.WrittenCount];
            this.e2ee.DecryptFrame(voiceUser.UserId, decryptedWriter.WrittenSpan, audio);

            this.Receiver.IngestAudio(frameInfo.SSRC, frameInfo.Timestamp, frameInfo.Sequence, audio);
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

        AudioBufferLease lease = await this.sendingAudioChannel.Reader.ReadAsync();
        int length = WriteAndEncryptFrame(lease.Buffer, currentFrame, timestamp, sequence);

        // return this lease to the pool as soon as we can
        lease.Dispose();

        while (await timer.WaitForNextTickAsync(ct))
        {
            unchecked { timestamp += 20; }

            // we were processing a long frame and haven't yet waited long enough, just wait until the next tick
            if (--counter > 0)
            {
                continue;
            }

            // increment this after the early return, unlike timestamp
            unchecked { sequence++; }

            // we are due to send the next frame
            if (framePrepared)
            {
                if (!this.IsSpeaking)
                {
                    await StartSpeakingAsync();
                }

                silenceCounter = 0;
                await this.mediaTransport.SendAsync(currentFrame.AsMemory()[..length]);
            }
            else
            {
                // only send up to five silence frames, after that we're interpreted to have shut up and don't need
                // to send any more silence
                if (silenceCounter < 5)
                {
                    silenceCounter++;
                    await SendSilenceFrame();
                }
                else if (silenceCounter == 5)
                {
                    await StopSpeakingAsync();
                }
            }

            // prepare and encrypt the next frame
            
            // if we don't have audio waiting, send silence
            if (!this.sendingAudioChannel.Reader.TryRead(out lease))
            {
                framePrepared = false;
                continue;
            }

            counter = lease.FrameCount;
            length = WriteAndEncryptFrame(lease.Buffer, currentFrame, timestamp, sequence);
            lease.Dispose();
        }

        async Task SendSilenceFrame()
        {
            lease = this.encoder.WriteSilenceFrame();
            length = WriteAndEncryptFrame(lease.Buffer, currentFrame, timestamp, sequence);
            lease.Dispose();

            await this.mediaTransport.SendAsync(currentFrame.AsMemory()[..length]);
        }
    }
    
    private int WriteAndEncryptFrame(ReadOnlySpan<byte> unencrypted, Span<byte> target, uint timestamp, ushort sequence)
    {
        ArrayPoolBufferWriter<byte> e2eeWriter = new();
        ArrayPoolBufferWriter<byte> encryptedWriter = new();

        this.e2ee.EncryptFrame(unencrypted, e2eeWriter);

        RTPHelper.WriteRtpHeader(encryptedWriter.GetSpan(12), sequence, timestamp, this.ssrc);
        encryptedWriter.Advance(12);

        this.cryptor.Encrypt(e2eeWriter.WrittenSpan, encryptedWriter);

        encryptedWriter.WrittenSpan.CopyTo(target);
        return encryptedWriter.WrittenCount;
    }
}
