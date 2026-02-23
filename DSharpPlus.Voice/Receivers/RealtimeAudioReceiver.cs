using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;

using CommunityToolkit.HighPerformance;

using DSharpPlus.Voice.Codec;
using DSharpPlus.Voice.MemoryServices;

namespace DSharpPlus.Voice.Receivers;

internal sealed class RealtimeAudioReceiver : IUserAudioReceiver
{
    private static readonly byte[] silentFrame = new byte[3840];
    private static readonly byte[] silentMillisecond = new byte[192];

    private readonly IAudioDecoder decoder;
    
    private readonly Pipe pipe;
    private readonly SortedDictionary<ushort, (uint, byte[])> queuedPackets;
    private readonly Lock outOfOrderFixupLock;
    private int silentCounter;
    private long lastTimestamp;
    private int timestampOverflowCounter;
    private ushort highestReceivedSequence;
    private ushort lastWrittenSequence;

    public RealtimeAudioReceiver(IAudioDecoder decoder)
    {
        this.pipe = new();
        this.queuedPackets = [];
        this.outOfOrderFixupLock = new();
        this.decoder = decoder;
    }

    /// <inheritdoc />
    public PipeReader Reader => this.pipe.Reader;

    /// <inheritdoc />
    public bool IsSpeaking { get; }

    /// <inheritdoc />
    public void Ingest(ushort sequence, uint timestamp, byte[] audio)
    {
        // start by calculating the timestamp we're working with, accounting for overflows. unfortunately, WebRTC, in its infinite wisdom,
        // does not use 64 bits for millisecond-precision timestamps. the largest possible opus packet is 120, but theoretically we could be
        // behind by more than a few packets - just test by some outrageously long duration that surely we won't ever be behind be unless
        // much more serious issues are at play, like... a minute
        if (timestamp < this.lastTimestamp - (uint.MaxValue - 60000))
        {
            this.timestampOverflowCounter++;
        }

        long expandedTimestamp = ((long)uint.MaxValue * this.timestampOverflowCounter) + timestamp;

        // out of order sequence received
        if (sequence < this.highestReceivedSequence && (this.highestReceivedSequence != ushort.MaxValue || sequence != 0))
        {
            lock (this.outOfOrderFixupLock)
            {
                List<(uint, byte[])> packets = [];

                for (ushort i = (ushort)(this.lastWrittenSequence + 1); i <= this.highestReceivedSequence; i++)
                {
                    if (i == sequence)
                    {
                        packets.Add((timestamp, audio));
                    }
                    else if (this.queuedPackets.TryGetValue(i, out (uint, byte[]) value))
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

                foreach ((uint _, byte[] audioPacket) in packets)
                {
                    this.decoder.Decode(audioPacket, new PipeWriterInt16BufferWriter(this.pipe.Writer));
                }

                this.lastWrittenSequence = sequence;
                this.highestReceivedSequence = sequence;

                return;
            }
        }
        
        // silence received
        if (audio is [0xF8, 0xFF, 0xFE])
        {
            this.silentCounter++;
            long lastKnownTimestamp = this.lastTimestamp;
            this.lastTimestamp = expandedTimestamp;

            this.pipe.Writer.Write(silentFrame);
            
            // a silence frame is 20ms, but discord is stupid and allows sending fewer of them without any formal warning after the fifth
            if (this.silentCounter >= 5)
            {
                if (lastKnownTimestamp < expandedTimestamp - 20)
                {
                    for (long i = lastKnownTimestamp; i < expandedTimestamp - 20; i++)
                    {
                        this.pipe.Writer.Write(silentMillisecond);
                    }
                }
            }
        }
        // audio received
        else
        {
            this.decoder.Decode(audio, new PipeWriterInt16BufferWriter(this.pipe.Writer));
            this.lastWrittenSequence = sequence;
            this.highestReceivedSequence = sequence;
            this.lastTimestamp = expandedTimestamp;
        }

        this.lastTimestamp = expandedTimestamp;
    }

    /// <inheritdoc />
    public void Close() 
        => this.pipe.Reader.Complete();
}
