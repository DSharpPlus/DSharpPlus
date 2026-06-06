using System.Collections.Generic;

namespace DSharpPlus.Voice.Protocol.RTCP.Payloads;

/// <summary>
/// Represents a receiver report packet.
/// </summary>
internal sealed record RTCPReceiverReportPacket : IRTCPPacket
{
    /// <inheritdoc/>
    public RTCPPacketType Type => RTCPPacketType.ReceiverReport;

    /// <summary>
    /// The SSRC of the reporting user.
    /// </summary>
    public required uint SSRC { get; init; }

    /// <summary>
    /// Reports of how well the reception from other users has been since sending the last sender/receiver report packet.
    /// </summary>
    public required IReadOnlyList<ReceptionReport> ReceptionReports { get; init; }
}
