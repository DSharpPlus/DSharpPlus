using System;

namespace DSharpPlus.Voice.Protocol.RTP;

internal record struct RTPTimestamp
{
    private readonly DateTimeOffset startTime;
    private ulong milliseconds;
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
        this.milliseconds = (ulong)(currentTime - this.startTime).TotalMilliseconds;
    }

    /// <summary>
    /// Gets the truncated value of the currently stored RTP timestamp.
    /// </summary>
    public readonly uint Value => (uint)(this.milliseconds + this.startingOffset);

    /// <summary>
    /// Gets the current time as exact RTP timestamp.
    /// </summary>
    public readonly uint ExactValue => (uint)((ulong)(DateTimeOffset.UtcNow - this.startTime).TotalMilliseconds + this.startingOffset);

    /// <summary>
    /// Adds to the current RTP timestamp.
    /// </summary>
    public void Add(uint milliseconds)
        => this.milliseconds += milliseconds;
}
