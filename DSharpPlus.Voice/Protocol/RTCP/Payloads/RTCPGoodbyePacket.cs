using System.Collections.Generic;

namespace DSharpPlus.Voice.Protocol.RTCP.Payloads;

/// <summary>
/// Represents a goodbye packet.
/// </summary>
internal sealed record RTCPGoodbyePacket : IRTCPPacket
{
    public RTCPPacketType Type => RTCPPacketType.Goodbye;

    /// <summary>
    /// The SSRCs of users leaving the call.
    /// </summary>
    public required IReadOnlyList<uint> SSRCs { get; init; }

    /// <summary>
    /// An optional reason for leaving.
    /// </summary>
    public string? Reason { get; init; }
}
