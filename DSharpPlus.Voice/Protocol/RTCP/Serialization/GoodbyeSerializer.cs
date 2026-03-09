using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Protocol.RTCP.Payloads;

namespace DSharpPlus.Voice.Protocol.RTCP.Serialization;

/// <summary>
/// Provides a mechanism to serialize and deserialize <see cref="RTCPGoodbyePacket"/>s. 
/// </summary>
internal static class GoodbyeSerializer
{
    public static void Serialize(RTCPGoodbyePacket packet, ArrayPoolBufferWriter<byte> writer)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(packet.SSRCs.Count, 31, "RTCPGoodbyePacket.SSRCs.Count");
        ArgumentOutOfRangeException.ThrowIfEqual(packet.SSRCs.Count, 0, "RTCPGoodbyePacket.SSRCs.Count");

        // the first three bits are always constant, version 2 (10) and no padding (0).
        byte firstHeaderByte = (byte)(0b100_00000 | (packet.SSRCs.Count & 0b000_11111));
        byte secondHeaderByte = (byte)RTCPPacketType.Goodbye;

        // +1 for length, +3  for mandatory padding to a multiple of four (the remainder gets cut off by the idiv anyway)
        ushort packetLength = (ushort)(packet.SSRCs.Count + packet.Reason is not null
            ? (Encoding.UTF8.GetByteCount(packet.Reason!) + 1 + 3) / 4
            : 0);

        Span<byte> main = writer.GetSpan(4 + (packet.SSRCs.Count * 4));

        main[0] = firstHeaderByte;
        main[1] = secondHeaderByte;
        BinaryPrimitives.WriteUInt16BigEndian(main[2..], packetLength);

        int written = 4;

        foreach (uint ssrc in packet.SSRCs)
        {
            BinaryPrimitives.WriteUInt32BigEndian(main[written..], ssrc);
            written += 4;
        }

        writer.Advance(written);

        if (packet.Reason is not null)
        {
            int length = Encoding.UTF8.GetByteCount(packet.Reason);

            ArgumentOutOfRangeException.ThrowIfGreaterThan(length, 255, "RTCPGoodbyePacket.Reason.Length");

            writer.Write((byte)length);

            Span<byte> reasonSpan = writer.GetSpan(length);
            Encoding.UTF8.GetBytes(packet.Reason, reasonSpan);
            writer.Advance(length);

            // we only need to fix up padding if we're writing a reason, otherwise, it'll already be padded correctly
            written = writer.WrittenCount;
            int target = (written + 3) & ~0b11;
            int diff = target - written;

            Span<byte> padding = writer.GetSpan(diff);
            padding.Clear();
            writer.Advance(diff);
        }
    }

    public static RTCPGoodbyePacket Deserialize(ReadOnlySpan<byte> packet, out int consumed)
    {
        Debug.Assert(packet[1] == (byte)RTCPPacketType.Goodbye);
        ArgumentOutOfRangeException.ThrowIfNotEqual(packet[0] & 0b11000000, 0b10000000, "RTCP packet version");

        ushort length = BinaryPrimitives.ReadUInt16BigEndian(packet[2..]);
        int ssrcCount = packet[0] & 0b00011111;

        List<uint> ssrcs = new(ssrcCount);
        string? reason = null;

        consumed = 4;

        for (int i = 0; i < ssrcCount; i++)
        {
            ssrcs.Add(BinaryPrimitives.ReadUInt32BigEndian(packet[consumed..]));
            consumed += 4;
        }

        // if there's more data remaining, there's a reason provided
        if (consumed / 4 != length + 1)
        {
            byte reasonLength = packet[consumed++];
            reason = Encoding.UTF8.GetString(packet[consumed..(consumed + reasonLength)]);
        }

        consumed = (length + 1) * 4;

        return new()
        {
            SSRCs = ssrcs,
            Reason = reason
        };
    }
}
