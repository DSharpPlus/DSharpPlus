using System;

namespace DSharpPlus.Voice.Protocol.RTP;

internal record struct RTPTimestamp
{
    private readonly DateTimeOffset startTime;
    private ulong ticks;
    private readonly uint startingOffset;

    public RTPTimestamp()
    {
        this.startingOffset = (uint)Random.Shared.NextInt64();
        this.startTime = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Realigns the RTP timestamp with the current time after a gap in transmission.
    /// </summary>
    public void RealignTimestamp()
    {
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        this.ticks = (ulong)(currentTime - this.startTime).TotalMilliseconds * 48;
    }

    /// <summary>
    /// Gets the truncated value of the currently stored RTP timestamp.
    /// </summary>
    public readonly uint Value => (uint)(this.ticks + this.startingOffset);

    /// <summary>
    /// Gets the current time as exact RTP timestamp.
    /// </summary>
    public readonly uint ExactValue => (uint)(((ulong)(DateTimeOffset.UtcNow - this.startTime).TotalMilliseconds * 48) + this.startingOffset);

    /// <summary>
    /// Adds to the current RTP timestamp.
    /// </summary>
    public void Add(uint ticks)
        => this.ticks += ticks;
}
