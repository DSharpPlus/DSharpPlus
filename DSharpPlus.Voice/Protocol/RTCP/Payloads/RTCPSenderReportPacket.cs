using System;
using System.Collections.Generic;

namespace DSharpPlus.Voice.Protocol.RTCP.Payloads;

/// <summary>
/// Represents a sender report packet.
/// </summary>
internal sealed record RTCPSenderReportPacket : IRTCPPacket
{
    /// <inheritdoc/>
    public RTCPPacketType Type => RTCPPacketType.SenderReport;

    /// <summary>
    /// The SSRC of the user sending this report.
    /// </summary>
    public required uint SSRC { get; init; }

    /// <summary>
    /// The time at which this report was sent.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// The RTP timestamp at which this report was sent, relative to the RTP timestamps sent with audio packets.
    /// </summary>
    public required uint RTPTimestamp { get; init; }

    /// <summary>
    /// The amount of packets sent by this sender across the connection lifetime.
    /// </summary>
    public required uint PacketsSent { get; init; }

    /// <summary>
    /// The amount of payload bytes sent by this sender across the connection lifetime.
    /// </summary>
    /// <remarks>
    /// This excludes RTCP packets, keepalive packets or RTP headers, merely the amount of opus bytes.
    /// </remarks>
    public required uint BytesSent { get; init; }

    /// <summary>
    /// Reports of how well the reception from other users has been since sending the last sender/receiver report packet.
    /// </summary>
    public required IReadOnlyList<ReceptionReport> ReceptionReports { get; init; }
}
