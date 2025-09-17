using System;
using System.Buffers;

using DSharpPlus.Voice.RuntimeServices.Memory;

namespace DSharpPlus.Voice.RuntimeServices.Pipes;

/// <summary>
/// Represents a segment of a s16le 48khz pcm buffer worth 1.2 seconds, or 10 long frames/60 short frames.
/// </summary>
public sealed class PcmAudioSegment : ReadOnlySequenceSegment<Int16x2>
{
    private readonly Int16x2[] array;
    private PcmAudioSegment? next;
    private int currentIndex;

    public bool IsFull => this.currentIndex + 1 == this.array.Length;

    public bool IsFinalSegment => this.next is null;

    public long NextRunningIndex => this.RunningIndex + 57600;

    public long RunningHeadIndex => this.RunningIndex + this.currentIndex;

    public PcmAudioSegment EndOfChain
    {
        get
        {
            PcmAudioSegment segment = this;

            while (segment.next is not null)
            {
                segment = segment.next;
            }

            return segment;
        }
    }

    public PcmAudioSegment(long running)
    {
        this.array = new Int16x2[57600];
        this.Memory = this.array.AsMemory();

        this.next = null;
        this.Next = null;

        this.RunningIndex = running;
    }

    /// <summary>
    /// Copies as much data as possible from the provided span, returning how much was written.
    /// </summary>
    /// <remarks>
    /// This does not deal with creating a new linked list entry, which needs to be done by the caller and then set appropriately.
    /// </remarks>
    internal int Copy(ReadOnlySpan<Int16x2> buffer)
    {
        Span<Int16x2> target = this.array.AsSpan()[this.currentIndex..];
        buffer.CopyTo(target);

        int written = int.Min(buffer.Length, target.Length);
        this.currentIndex += written;

        return written;
    }

    /// <summary>
    /// Sets the provided segment as the next entry in the linked list.
    /// </summary>
    internal PcmAudioSegment SetNextSegment(PcmAudioSegment next)
    {
        this.Next = next;
        return this.next = next;
    }
}
