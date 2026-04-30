using System.Collections.Generic;

namespace DSharpPlus.Voice.Protocol.RTCP.Payloads;

/// <summary>
/// Represents a source description packet.
/// </summary>
internal sealed record RTCPSourceDescriptionPacket : IRTCPPacket
{
    /// <inheritdoc/>
    public RTCPPacketType Type => RTCPPacketType.SourceDescription;

    /// <summary>
    /// Collections of description items from each contributing source.
    /// </summary>
    public required IReadOnlyList<SourceDescription> SourceDescriptions { get; init; }
}
