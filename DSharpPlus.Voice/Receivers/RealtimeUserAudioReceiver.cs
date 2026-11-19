using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance;

using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.MemoryServices;

namespace DSharpPlus.Voice.Receivers;

internal sealed class RealtimeUserAudioReceiver : UserAudioReceiver
{
    private static readonly byte[] silentFrame = new byte[3840];
    private static readonly byte[] silentMillisecond = new byte[192];

    private readonly IAudioDecoder decoder;
    
    private readonly Pipe pipe;
    private readonly SortedDictionary<uint, (AudioTimestamp, byte[])> queuedPackets;
    private readonly SemaphoreSlim outOfOrderFixupLock;
    private int silentCounter;
    private AudioTimestamp lastTimestamp;
    private uint lastWrittenSequence;
    private uint highestReceivedSequence;

    public RealtimeUserAudioReceiver(IAudioDecoder decoder)
    {
        this.pipe = new();
        this.queuedPackets = [];
        this.outOfOrderFixupLock = new(1, 1);
        this.decoder = decoder;
    }

    /// <inheritdoc />
    public override PipeReader Reader => this.pipe.Reader;

    /// <inheritdoc />
    protected internal override async ValueTask IngestAsync(uint sequence, AudioTimestamp timestamp, TimeSpan duration, byte[] audio)
    {
        // out of order sequence received
        if (sequence < this.highestReceivedSequence)
        {
            this.IsSpeaking = true;

            await this.outOfOrderFixupLock.WaitAsync();
            try
            {
                List<(AudioTimestamp, byte[])> packets = [];

                for (ushort i = (ushort)(this.lastWrittenSequence + 1); i <= this.highestReceivedSequence; i++)
                {
                    if (i == sequence)
                    {
                        packets.Add((timestamp, audio));
                    }
                    else if (this.queuedPackets.TryGetValue(i, out (AudioTimestamp, byte[]) value))
                    {
                        packets.Add(value);
                    }
                    else
                    {
                        // we have a hole
                        this.queuedPackets.Add(sequence, (timestamp, audio));
                        this.highestReceivedSequence = sequence;
                        return;
                    }
                }

                foreach ((AudioTimestamp _, byte[] audioPacket) in packets)
                {
                    this.decoder.Decode(audioPacket, new PipeWriterInt16BufferWriter(this.pipe.Writer));
                }

                await this.pipe.Writer.FlushAsync();

                this.lastWrittenSequence = sequence;
                this.highestReceivedSequence = sequence;

                return;
            }
            finally
            {
                this.outOfOrderFixupLock.Release();
            }
        }
        
        // silence received
        if (audio is [0xF8, 0xFF, 0xFE])
        {
            this.silentCounter++;
            AudioTimestamp lastKnownTimestamp = this.lastTimestamp;
            this.lastTimestamp = timestamp;

            await this.pipe.Writer.WriteAsync(silentFrame);
            
            // a silence frame is 20ms, but discord is stupid and allows sending fewer of them without any formal warning after the fifth
            if (this.silentCounter >= 5)
            {
                this.IsSpeaking = false;

                if (lastKnownTimestamp < timestamp - TimeSpan.FromMilliseconds(20))
                {
                    for (AudioTimestamp t = lastKnownTimestamp; t < timestamp - TimeSpan.FromMilliseconds(20); t += TimeSpan.FromMilliseconds(1))
                    {
                        await this.pipe.Writer.WriteAsync(silentMillisecond);
                    }
                }
            }
        }
        // audio received
        else
        {
            this.IsSpeaking = true;
            this.decoder.Decode(audio, new PipeWriterInt16BufferWriter(this.pipe.Writer));
            this.lastWrittenSequence = sequence;
            this.highestReceivedSequence = sequence;
        }
        
        await this.pipe.Writer.FlushAsync();

        this.lastTimestamp = timestamp;
    }

    /// <inheritdoc />
    protected internal override void Close() 
        => this.pipe.Reader.Complete();
}
