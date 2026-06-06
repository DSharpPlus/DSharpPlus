namespace DSharpPlus.Voice.Protocol.RTCP.Payloads;

/// <summary>
/// Represents an application-defined packet.
/// </summary>
internal sealed record RTCPApplicationDefinedPacket : IRTCPPacket
{   
    /// <inheritdoc/>
    public RTCPPacketType Type => RTCPPacketType.ApplicationDefined;

    /// <summary>
    /// An application-defined subtype, between 0 and 31.
    /// </summary>
    public required int Subtype { get; init; }

    /// <summary>
    /// The SSRC of the transmitting user.
    /// </summary>
    public required uint SSRC { get; init; }

    /// <summary>
    /// The application-defined name of this packet. This must be exactly four characters of plain ASCII.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Application-defined data, may be missing.
    /// </summary>
    public byte[]? Data { get; init; }
}
