using System;
using System.Buffers.Binary;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Protocol.RTCP.Payloads;

namespace DSharpPlus.Voice.Protocol.RTCP.Serialization;

internal static class ReceptionReportSerializer
{
    public static void Serialize(ReceptionReport report, ArrayPoolBufferWriter<byte> writer)
    {
        // this word is laid out as such: 8 bits packet loss, 24 bits cumulative packets lost, big endian
        uint packetLossWord = (report.CumulativePacketsLost & 0x00FFFFFF) | (uint)((byte)(report.PacketLoss * 256) << 24);

        // the delay is expressed in the 65536th part of a second
        uint delay = (uint)((double)report.DelaySinceLastSenderReport.TotalMicroseconds / 1000000 * 65536);

        Span<byte> reportSpan = writer.GetSpan(24);

        BinaryPrimitives.WriteUInt32BigEndian(reportSpan, report.SSRC);
        BinaryPrimitives.WriteUInt32BigEndian(reportSpan[4..], packetLossWord);
        BinaryPrimitives.WriteUInt32BigEndian(reportSpan[8..], report.HighestSequenceReceived);
        BinaryPrimitives.WriteUInt32BigEndian(reportSpan[12..], report.InterarrivalJitter);
        BinaryPrimitives.WriteUInt32BigEndian(reportSpan[16..], report.LastSenderReportTimestamp.ToRFC3550CompactNTPTimestamp());
        BinaryPrimitives.WriteUInt32BigEndian(reportSpan[20..], delay);

        writer.Advance(24);
    }

    public static ReceptionReport Deserialize(ReadOnlySpan<byte> buffer, out int consumed)
    {
        uint sourceSSRC = BinaryPrimitives.ReadUInt32BigEndian(buffer);
        uint packetLossWord = BinaryPrimitives.ReadUInt32BigEndian(buffer[4..]);
        uint highestSequence = BinaryPrimitives.ReadUInt32BigEndian(buffer[8..]);
        uint jitter = BinaryPrimitives.ReadUInt32BigEndian(buffer[12..]);
        uint lastSenderReport = BinaryPrimitives.ReadUInt32BigEndian(buffer[16..]);
        uint delay = BinaryPrimitives.ReadUInt32BigEndian(buffer[20..]);

        consumed = 24;

        float packetLoss = (float)((packetLossWord & 0xFF000000) >> 24) / 256;
        uint cumulativePacketsLost = packetLossWord & 0x00FFFFFF;

        DateTimeOffset lastSenderReportTimestamp = DateTimeOffset.FromRFC3550CompactNTPTimestamp(lastSenderReport);
        TimeSpan delaySinceLastSenderReport = TimeSpan.FromMicroseconds((double)delay / 65536 * 1000000);

        return new()
        {
            SSRC = sourceSSRC,
            PacketLoss = packetLoss,
            CumulativePacketsLost = cumulativePacketsLost,
            HighestSequenceReceived = highestSequence,
            InterarrivalJitter = jitter,
            LastSenderReportTimestamp = lastSenderReportTimestamp,
            DelaySinceLastSenderReport = delaySinceLastSenderReport
        };
    }
}
