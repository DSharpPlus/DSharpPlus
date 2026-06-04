#pragma warning disable IDE0040, IDE0045

using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.MemoryServices;
using DSharpPlus.Voice.MemoryServices.Channels;
using DSharpPlus.Voice.Protocol.RTP;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice;

partial class VoiceConnection
{
    private async Task SendAudioAsync(CancellationToken ct)
    {
        ushort sequence = (ushort)Random.Shared.NextInt64();

        while (!ct.IsCancellationRequested)
        {
            if (this.selfMute)
            {
                await this.noLongerSelfMuted.Task;
            }

            if (this.pauseTransmissionWhenAlone && this.connectedUsers.Count == 0)
            {
                await this.noLongerAlone.Task;
            }

            await this.sendingAudioChannel.Reader.WaitToReadAsync(ct);
            this.timestamp.RealignTimestamp();

            // here we select the optimal way to send audio. since e2ee requires us to stay at realtime, and e2ee simultaneously
            // takes comparatively a lot of time, SendNarrowlyTimedAudioAsync is engineered to do this best; if we have enough
            // audio available to us. unfortunately, the approach it takes is very irresilient to audio being provided in
            // realtime and tiny variation in timing can cascade into major issues when it thinks to detect silence but actually
            // just detects 5µs microstutter that will average out to zero.
            // our second send system, SendBroadlyTimedAudioAsync, is less performant at scale and struggles more with keeping in
            // sync with realtime, so the inner send loop is a bit of a ping-pong operation between those two modes.

            // audio was provided too quickly, switch to narrowly timed mode so we can stick close to realtime
            if (this.isAudioProvidedFasterThanRealtime)
            {
                this.isAudioProvidedFasterThanRealtime = false;
                sequence = await SendNarrowlyTimedAudioAsync(sequence, ct);
            }
            // audio was provided in Probably Realtime with a standard deviation from exact 20ms intervals detectable in narrowly
            // timed mode, switch to broad mode so the differences affect us less, but only if the user didn't just submit a huge
            // block of audio, because that would be catastrophic for trying to stay close to real-time.
            else if ((this.isAudioProbablyRealtime && this.sendingAudioChannel.QueueLength < 5) || this.isAudioSpiraling)
            {
                this.isAudioProbablyRealtime = false;
                this.isAudioSpiraling = false;
                sequence = await SendBroadlyTimedAudioAsync(sequence, ct);
            }
            // if none of the other reasons to choose a send mode apply, default to choosing by specified audiotype:
            // Realtime -> BroadlyTimed
            // Music/Auto -> NarrowlyTimed
            else if (this.audioType == AudioType.Realtime)
            {
                sequence = await SendBroadlyTimedAudioAsync(sequence, ct);
            }
            else
            {
                sequence = await SendNarrowlyTimedAudioAsync(sequence, ct);
            }
        }
    }
    
    private async Task<ushort> SendNarrowlyTimedAudioAsync(ushort sequence, CancellationToken ct)
    {
        int silenceCounter = 0;
        bool framePrepared = true;
        int length;
        byte[] currentFrame = ArrayPool<byte>.Shared.Rent(GetMaxEncryptedLength(5760));
        PeriodicTimer timer = new(TimeSpan.FromMilliseconds(20));

        int framesSent = 0;
        int readMisses = 0;
        int narrowReadMisses = 0;

        AudioChannelReadResult readResult = await this.sendingAudioChannel.Reader.ReadAsync(ct);

        if (readResult.State == AudioChannelState.Terminated)
        {
            return sequence;
        }

        AudioBufferLease lease = readResult.Buffer.Value;
        length = WriteAndEncryptFrame(lease.Buffer, currentFrame, this.timestamp.Value, sequence);
        lease.Dispose();

        // we have audio available, start sending it...
        while (await timer.WaitForNextTickAsync(ct))
        {
            // ...unless nobody is in the call and we're set to not consume audio if there are no recipients
            if (this.pauseTransmissionWhenAlone && this.connectedUsers.Count == 0)
            {
                break;
            }

            this.timestamp.Add(960);

            // this must be incremented after the early return, otherwise we produce wrong sequences here
            _ = unchecked(sequence++);

            if (framePrepared)
            {
                if (!this.IsSpeaking && this.isReady)
                {
                    this.logger.LogTrace("Commencing audio transmission to the remote server.");
                    await StartSpeakingAsync();
                }

                silenceCounter = 0;
                this.packetsSent++;
                this.opusBytesSent += (uint)length - 12;

                // don't waste bandwidth when there's nobody to hear us
                if (this.connectedUsers.Count != 0)
                {
                    await this.mediaTransport.SendAsync(currentFrame.AsMemory()[..length]);
                }

                this.metrics.RecordAudioFrameSent(length);

                framesSent++;
            }
            else
            {
                silenceCounter++;
                readMisses++;
            }

            // if the silence counter has exceeded the configured maximum, fall silent and pause
            if (silenceCounter >= this.globalOptions.SilenceTicksBeforePausingConnection)
            {
                sequence = await SendFiveSilenceFramesAsync(sequence);
                break;
            }

            // if more than 50% of read misses are narrow (only missed by at most one tick), and misses amount to more than 2.5% of
            // total reads, it's reasonable to assume we're being provided with realtime audio
            // ensure we also have a somewhat statistically significant amount of audio to send, though
            if (narrowReadMisses >= readMisses / 2 && readMisses >= framesSent / 40 && framesSent > 80)
            {
                this.logger.LogTrace("Switching to broadly-timed sending because of too many narrow timing misses.");

                this.isAudioProbablyRealtime = true;
                break;
            }

            // to keep the read/write statistics up to date, reset them every 500 frames (10s)
            if (framesSent > 500)
            {
                framesSent = 0;
                readMisses = 0;
                narrowReadMisses = 0;
            }

            TimeSpan delay = this.timestamp.GetDelayToRealtime();

            // if we're falling behind, enter a catchup loop
            if (delay >= this.globalOptions.ConsiderConnectionDelayedThreshold)
            {
                if (delay >= 2 * this.globalOptions.ConsiderConnectionDelayedThreshold)
                {
                    this.logger.LogWarning("The audio sending loop is running {time} - is the bot overloaded?", FormatDelay(delay));
                }
                else
                {
                    this.logger.LogTrace("The audio sending loop is running {time}, entering catchup loop.", FormatDelay(delay));
                }

                while (delay > TimeSpan.FromMilliseconds(20))
                {
                    readResult = await this.sendingAudioChannel.Reader.ReadAsync(ct);

                    if (!readResult.IsAvailable)
                    {
                        // if we're told to not expect any audio soon, just shut up
                        if (readResult.State is AudioChannelState.Paused or AudioChannelState.Terminated)
                        {
                            sequence = await SendFiveSilenceFramesAsync(sequence);
                            break;
                        }

                        continue;
                    }

                    lease = readResult.Buffer.Value;

                    length = WriteAndEncryptFrame(lease.Buffer, currentFrame, this.timestamp.Value, sequence);
                    lease.Dispose();

                    this.timestamp.Add(960);
                    _ = unchecked(sequence++);

                    this.packetsSent++;
                    this.opusBytesSent += (uint)length - 12;
                    await this.mediaTransport.SendAsync(currentFrame.AsMemory()[..length]);

                    this.metrics.RecordAudioFrameSent(length);
                }
            }

            // otherwise, let's go and encrypt the next bit of audio
            readResult = this.sendingAudioChannel.Reader.TryRead();

            if (!readResult.IsAvailable)
            {
                framePrepared = false;

                // if we're told to not expect any audio soon, just shut up
                if (readResult.State is AudioChannelState.Paused or AudioChannelState.Terminated)
                {
                    sequence = await SendFiveSilenceFramesAsync(sequence);
                    break;
                }
                
                continue;
            }

            if (silenceCounter == 1)
            {
                Console.WriteLine("narrowly missed");
                narrowReadMisses++;
            }

            lease = readResult.Buffer.Value;

            length = WriteAndEncryptFrame(lease.Buffer, currentFrame, this.timestamp.Value, sequence);
            lease.Dispose();

            framePrepared = true;
        }

        ArrayPool<byte>.Shared.Return(currentFrame);
        return sequence;
    }

    private async Task<ushort> SendBroadlyTimedAudioAsync(ushort sequence, CancellationToken ct)
    {
        byte[] currentFrame = ArrayPool<byte>.Shared.Rent(GetMaxEncryptedLength(5760));
        DateTimeOffset lastLoggedTimestamp = DateTimeOffset.UtcNow;
        TimeSpan timeToNextLogCheck = TimeSpan.FromSeconds(5);
        int iterationsSinceLastResynchronizing = 0;
        int consecutiveTimingChecksFailed = 0;
        TimeSpan lastDelay = TimeSpan.Zero;

        while (true)
        {
            AudioChannelReadResult readResult = await this.sendingAudioChannel.Reader.ReadAsync(ct);

            if (!readResult.IsAvailable)
            {
                // if we're told to not expect any audio soon, just shut up
                if (readResult.State is AudioChannelState.Paused or AudioChannelState.Terminated)
                {
                    sequence = await SendFiveSilenceFramesAsync(sequence);
                    break;
                }

                continue;
            }

            AudioBufferLease lease = readResult.Buffer.Value;
            int length = WriteAndEncryptFrame(lease.Buffer, currentFrame, this.timestamp.Value, sequence);
            lease.Dispose();

            if (!this.IsSpeaking && this.isReady)
            {
                this.logger.LogTrace("Commencing audio transmission to the remote server.");
                await StartSpeakingAsync();
            }

            this.packetsSent++;
            this.opusBytesSent += (uint)length - 12;

            // don't waste bandwidth when there's nobody to hear us
            if (this.connectedUsers.Count != 0)
            {
                await this.mediaTransport.SendAsync(currentFrame.AsMemory()[..length]);
            }

            this.metrics.RecordAudioFrameSent(length);

            _ = unchecked(sequence++);
            this.timestamp.Add(960);

            // if we're too far behind, log that; if we're too far ahead, break and switch mode
            TimeSpan delay = this.timestamp.GetDelayToRealtime();

            if (delay > this.globalOptions.ConsiderConnectionDelayedThreshold)
            {
                // don't flood logs
                DateTimeOffset now = DateTimeOffset.UtcNow;
                
                if (now - timeToNextLogCheck > lastLoggedTimestamp)
                {
                    this.logger.LogDebug("The audio sending loop is running {time} - is the bot overloaded?", FormatDelay(delay));
                    lastLoggedTimestamp = now;

                    timeToNextLogCheck += TimeSpan.FromSeconds(5);

                    if (timeToNextLogCheck > TimeSpan.FromSeconds(30))
                    {
                        timeToNextLogCheck = TimeSpan.FromSeconds(30);
                    }

                    // this serves as our cue that we need to resynchronize: for ten consecutive times, we fell further behind
                    if (consecutiveTimingChecksFailed > 8 && delay > lastDelay)
                    {
                        this.logger.LogWarning("Resynchronizing the audio sending loop, it seems to be spiraling behind");

                        this.isAudioSpiraling = true;
                        break;
                    }

                    consecutiveTimingChecksFailed += (delay > lastDelay) ? 1 : 0;
                    lastDelay = delay;
                }
            }
            else if (delay < -this.globalOptions.ConsiderConnectionDelayedThreshold)
            {
                // if we experience slight drift we can just fix it with a Task.Delay, but if we experience statistically significant drift,
                // exit and switch mode, since at this point it's quite likely audio is just simply too fast for this mode to handle
                // 100 iterations is arbitrarily chosen, but that's two seconds, which should mean that if the user continues supplying audio
                // at this rate, there won't be much stutter when switching mode
                if (iterationsSinceLastResynchronizing < 100)
                {
                    this.logger.LogTrace("Switching to narrowly-timed sending because audio is consistently faster than realtime.");

                    this.isAudioProvidedFasterThanRealtime = true;
                    await Task.Delay(-delay, ct);
                    break;
                }
                else
                {
                    this.logger.LogTrace("Broadly-timed sending drifted ahead of realtime, resynchronizing.");

                    await Task.Delay(-delay, ct);
                    iterationsSinceLastResynchronizing = 0;
                }
            }
            else if (delay <= TimeSpan.FromMilliseconds(20))
            {
                // we evidently caught up, given we're not running more than a frame behind
                timeToNextLogCheck = TimeSpan.FromSeconds(5);
            }

            iterationsSinceLastResynchronizing++;
        }

        ArrayPool<byte>.Shared.Return(currentFrame);
        return sequence;
    }

    private async Task<ushort> SendFiveSilenceFramesAsync(ushort sequence)
    {
        this.logger.LogTrace("Pausing audio transmission to the remote server.");
        await StopSpeakingAsync();

        for (int i = 0; i < 5; i++)
        {
            AudioBufferLease lease = this.encoder.WriteSilenceFrame();
            byte[] currentFrame = new byte[GetMaxEncryptedLength(lease.Buffer.Length)];

            int length = WriteAndEncryptFrame(lease.Buffer, currentFrame, this.timestamp.Value, sequence);
            lease.Dispose();

            this.timestamp.Add(960);
            _ = unchecked(sequence++);

            this.packetsSent++;
            this.opusBytesSent += 3;
            await this.mediaTransport.SendAsync(currentFrame.AsMemory()[..length]);

            this.metrics.RecordAudioFrameSent(length);
        }

        return sequence;
    }
    
    private int WriteAndEncryptFrame(ReadOnlySpan<byte> unencrypted, Span<byte> target, uint timestamp, ushort sequence)
    {
        using ArrayPoolBufferWriter<byte> e2eeWriter = new();

        RTPHelper.WriteRTPHeader(e2eeWriter.GetSpan(12), sequence, timestamp, this.ssrc);
        e2eeWriter.Advance(12);

        // don't encrypt if we're on dave version 0, fallback to writing unencrypted if e2ee failed (race condition with the vgw)
        if (this.daveVersion == 0 || !this.e2ee.TryEncryptFrame(unencrypted, e2eeWriter))
        {
            e2eeWriter.Write(unencrypted);
        }
        
        using ArrayPoolBufferWriter<byte> encryptedWriter = new();

        this.cryptor.Encrypt(e2eeWriter.WrittenSpan, encryptedWriter);

        encryptedWriter.WrittenSpan.CopyTo(target);
        return encryptedWriter.WrittenCount;
    }

    /// <summary>
    /// Gets the maximum encrypted length of a packet after layering all stages of encryption together, given a particular unencrypted length.
    /// </summary>
    private int GetMaxEncryptedLength(int unencrypted)
        => this.cryptor.GetMaxEncryptedLength(this.e2ee.GetMaxEncryptedLength(unencrypted));

    private static string FormatDelay(TimeSpan time)
        => $"{long.Abs((long)time.TotalMilliseconds)}ms {(time > TimeSpan.Zero ? "behind" : "ahead")}";
}
