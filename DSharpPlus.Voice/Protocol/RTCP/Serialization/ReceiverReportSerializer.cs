using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Protocol.RTCP.Payloads;

namespace DSharpPlus.Voice.Protocol.RTCP.Serialization;

/// <summary>
/// Provides a mechanism to serialize and deserialize <see cref="RTCPReceiverReportPacket"/>s. 
/// </summary>
internal static class ReceiverReportSerializer
{
    public static void Serialize(RTCPReceiverReportPacket packet, ArrayPoolBufferWriter<byte> writer)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(packet.ReceptionReports.Count, 31, "RTCPReceiverReportPacket.ReceptionReports.Count");

        // the first three bits are always constant, version 2 (10) and no padding (0).
        byte firstHeaderByte = (byte)(0b100_00000 | (packet.ReceptionReports.Count & 0b000_11111));
        byte secondHeaderByte = (byte)RTCPPacketType.ReceiverReport;

        // the length is given in 32-bit words minus one: the packet including header is 2 words, but we subtract one; and each reception
        // report is 6 words. theoretically, padding ought to be included in this, but we don't need to pad our packets.
        ushort packetLength = (ushort)((packet.ReceptionReports.Count * 6) + 1);

        Span<byte> senderInfoSpan = writer.GetSpan(8);

        senderInfoSpan[0] = firstHeaderByte;
        senderInfoSpan[1] = secondHeaderByte;
        
        BinaryPrimitives.WriteUInt16BigEndian(senderInfoSpan[2..], packetLength);
        BinaryPrimitives.WriteUInt32BigEndian(senderInfoSpan[4..], packet.SSRC);

        writer.Advance(8);

        foreach (ReceptionReport report in packet.ReceptionReports)
        {
            ReceptionReportSerializer.Serialize(report, writer);
        }
    }

    public static RTCPReceiverReportPacket Deserialize(ReadOnlySpan<byte> packet, out int consumed)
    {
        Debug.Assert(packet[1] == (byte)RTCPPacketType.ReceiverReport);
        ArgumentOutOfRangeException.ThrowIfNotEqual(packet[0] & 0b11000000, 0b10000000, "RTCP packet version");

        bool hasPadding = (packet[0] & 0b00100000) == 0b00100000;
        ushort length = BinaryPrimitives.ReadUInt16BigEndian(packet[2..]);
        int receptionReports = packet[0] & 0b00011111;

        if (!hasPadding && length % 6 != 1)
        {
            throw new ArgumentOutOfRangeException
            (
                "RTCP Receiver Report packet length",
                $"Received an invalid RTCP packet length of {length + 1} words without declared padding."
            );
        }

        int expectedMinimumLength = (receptionReports * 6) + 1;

        ArgumentOutOfRangeException.ThrowIfLessThan(length, expectedMinimumLength, "RTCP Receiver Report packet length");

        // now that we validated the length, we can parse the sender report
        uint ssrc = BinaryPrimitives.ReadUInt32BigEndian(packet[4..]);

        consumed = 8;
        packet = packet[8..];

        // and the reception reports
        List<ReceptionReport> reports = new(receptionReports);

        for (int i = 0; i < receptionReports; i++)
        {
            ReceptionReport report = ReceptionReportSerializer.Deserialize(packet, out int reportConsumed);

            consumed += reportConsumed;
            packet = packet[reportConsumed..];

            reports.Add(report);
        }

        // if we have padding, we may not have consumed everything available, otherwise sanity-check ourselves
        if (!hasPadding)
        {
            Debug.Assert(consumed == (length + 1) * 4);
        }

        consumed = (length + 1) * 4;

        return new()
        {
            SSRC = ssrc,
            ReceptionReports = reports
        };
    }
}