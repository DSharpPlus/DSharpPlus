using System;

namespace DSharpPlus.Voice.Protocol.RTCP.Payloads;

/// <summary>
/// Contains information about signal reception from other users in the call.
/// </summary>
internal sealed record ReceptionReport
{
    /// <summary>
    /// The SSRC of the user this reception report concerns.
    /// </summary>
    public required uint SSRC { get; init; }

    /// <summary>
    /// The packet loss percentage from this user.
    /// </summary>
    public required float PacketLoss { get; init; }

    /// <summary>
    /// The total amount of packets lost from this user across the connection lifetime.
    /// </summary>
    public required uint CumulativePacketsLost { get; init; }

    /// <summary>
    /// The highest sequence number received from this user, extended to include rollovers.
    /// </summary>
    public required uint HighestSequenceReceived { get; init; }

    /// <summary>
    /// An estimate of the jitter in RTP packets' arrival time.
    /// </summary>
    public required uint InterarrivalJitter { get; init; }

    /// <summary>
    /// The timestamp at which the last sender report was received from this user.
    /// </summary>
    public required DateTimeOffset LastSenderReportTimestamp { get; init; }

    /// <summary>
    /// The delay since last receiving a sender report from this user.
    /// </summary>
    public required TimeSpan DelaySinceLastSenderReport { get; init; }
}
