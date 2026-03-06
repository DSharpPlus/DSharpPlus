using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.Cryptors;
using DSharpPlus.Voice.E2EE;
using DSharpPlus.Voice.MemoryServices;
using DSharpPlus.Voice.Protocol.Rtp;
using DSharpPlus.Voice.Transport;

namespace DSharpPlus.Voice;

/// <summary>
/// Handles communicating over voice.
/// </summary>
internal sealed class AudioClient : IDisposable
{
    private readonly IMediaTransportService mediaTransport;
    private readonly AudioReceiver receiver;
    private readonly ICryptor cryptor;
    private readonly IE2EESession e2ee;
    private readonly ChannelReader<AudioBufferLease> frameChannel;
    private readonly IAudioEncoder encoder;
    private readonly PeriodicTimer timer;
    private readonly VoiceConnection connection;
    
    private readonly Task sendTask;
    private readonly Task receiveTask;

    private readonly uint ssrc;

    public AudioClient
    (
        IMediaTransportService mediaTransport,
        AudioReceiver receiver,
        ICryptor cryptor,
        IE2EESession e2ee,
        ChannelReader<AudioBufferLease> frameChannel,
        IAudioEncoder encoder,
        VoiceConnection connection,
        uint ssrc
    )
    {
        this.mediaTransport = mediaTransport;
        this.receiver = receiver;
        this.cryptor = cryptor;
        this.e2ee = e2ee;
        this.frameChannel = frameChannel;
        this.encoder = encoder;
        this.timer = new(TimeSpan.FromMilliseconds(20));
        this.connection = connection;

        this.ssrc = ssrc;

        this.sendTask = SendLoopAsync();
        this.receiveTask = ReceiveLoopAsync();
    }

    private async Task SendLoopAsync()
    {
        int counter = 1;
        int silenceCounter = 0;
        bool framePrepared = true;
        byte[] currentFrame = new byte[5772]; // no real point pooling this, since we use it for the entire lifetime
        ushort sequence = (ushort)Random.Shared.NextInt64();
        uint timestamp = (uint)Random.Shared.NextInt64();

        AudioBufferLease lease = await this.frameChannel.ReadAsync();
        int length = WriteAndEncryptFrame(lease.Buffer.AsSpan()[..lease.Length], currentFrame, timestamp, sequence);

        // return this lease to the pool as soon as we can
        lease.Dispose();

        while (await this.timer.WaitForNextTickAsync())
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
                if (!this.connection.IsSpeaking)
                {
                    await this.connection.StartSpeakingAsync();
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
                    await this.connection.StopSpeakingAsync();
                }
            }

            // prepare and encrypt the next frame
            
            // if we don't have audio waiting, send silence
            if (!this.frameChannel.TryRead(out lease))
            {
                framePrepared = false;
                continue;
            }

            counter = lease.FrameCount;
            length = WriteAndEncryptFrame(lease.Buffer.AsSpan()[..lease.Length], currentFrame, timestamp, sequence);
            lease.Dispose();
        }

        async Task SendSilenceFrame()
        {
            lease = this.encoder.WriteSilenceFrame();
            length = WriteAndEncryptFrame(lease.Buffer.AsSpan()[..lease.Length], currentFrame, timestamp, sequence);
            lease.Dispose();

            await this.mediaTransport.SendAsync(currentFrame.AsMemory()[..length]);
        }
    }
    
    private int WriteAndEncryptFrame(Span<byte> unencrypted, Span<byte> target, uint timestamp, ushort sequence)
    {
        ArrayPoolBufferWriter<byte> e2eeWriter = new();
        ArrayPoolBufferWriter<byte> encryptedWriter = new();

        RtpHelper.WriteRtpHeader(e2eeWriter.GetSpan(12), sequence, timestamp, this.ssrc);
        e2eeWriter.Advance(12);

        this.e2ee.EncryptFrame(unencrypted, e2eeWriter);
        this.cryptor.Encrypt(e2eeWriter.WrittenSpan, encryptedWriter);

        encryptedWriter.WrittenSpan.CopyTo(target);
        return encryptedWriter.WrittenCount;
    }

    private async Task ReceiveLoopAsync()
    {
        while (true)
        {
            ArrayPoolBufferWriter<byte> receiveWriter = new();
            ArrayPoolBufferWriter<byte> decryptedWriter = new();

            await this.mediaTransport.ReceiveAsync(receiveWriter);

            this.cryptor.Decrypt(receiveWriter.WrittenSpan, decryptedWriter, out RtpFrameInfo frameInfo);
            
            if (!this.receiver.Ssrcs.TryGetValue(frameInfo.Ssrc, out VoiceUser? voiceUser))
            {
                // race condition: we're still waiting on their ssrc but they already started speaking
                // [TODO] consider buffering such received packets?
                continue;
            }

            // the transport cryptor already is supposed to only decrypt the audio data (the header counts as additional data)
            byte[] audio = new byte[decryptedWriter.WrittenCount];
            this.e2ee.DecryptFrame(voiceUser.UserId, decryptedWriter.WrittenSpan, audio);

            this.receiver.IngestAudio(frameInfo.Ssrc, frameInfo.Timestamp, frameInfo.Sequence, audio);
        }
    }

    public void Dispose()
    {
        // IMPORTANT: do not dispose any of the services passed into this type, they are managed by the
        // overall voice connection since they (most of them) find use elsewhere too
        this.sendTask.Dispose();
        this.receiveTask.Dispose();
    }
}