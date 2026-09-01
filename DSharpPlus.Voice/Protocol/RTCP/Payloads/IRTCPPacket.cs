namespace DSharpPlus.Voice.Protocol.RTCP.Payloads;

/// <summary>
/// Marker interface for RTCP packets.
/// </summary>
internal interface IRTCPPacket
{
    /// <summary>
    /// The type of this packet.
    /// </summary>
    public RTCPPacketType Type { get; }
}
