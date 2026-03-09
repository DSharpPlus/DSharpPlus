using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Text;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Protocol.RTCP.Payloads;

namespace DSharpPlus.Voice.Protocol.RTCP.Serialization;

/// <summary>
/// Provides a mechanism to serialize and deserialize <see cref="RTCPApplicationDefinedPacket"/>s. 
/// </summary>
internal static class ApplicationDefinedSerializer
{
    public static void Serialize(RTCPApplicationDefinedPacket packet, ArrayPoolBufferWriter<byte> writer)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(packet.Subtype, 31, "RTCPApplicationDefinedPacket.Subtype");
        ArgumentOutOfRangeException.ThrowIfLessThan(packet.Subtype, 0, "RTCPApplicationDefinedPacket.Subtype");
        ArgumentOutOfRangeException.ThrowIfNotEqual(Encoding.ASCII.GetByteCount(packet.Name), 4, "RTCPApplicationDefinedPacket.Name.Length");

        // the first three bits are always constant, version 2 (10) and no padding (0).
        byte firstHeaderByte = (byte)(0b100_00000 | (packet.Subtype & 0b000_11111));
        byte secondHeaderByte = (byte)RTCPPacketType.ApplicationDefined;
        ushort length = (ushort)(2 + ((packet.Data?.Length / 4) ?? 0));

        Span<byte> main = writer.GetSpan(12);
        
        main[0] = firstHeaderByte;
        main[1] = secondHeaderByte;

        BinaryPrimitives.WriteUInt16BigEndian(main[2..], length);
        BinaryPrimitives.WriteUInt32BigEndian(main[4..], packet.SSRC);
        Encoding.ASCII.GetBytes(packet.Name, main[8..12]);

        writer.Advance(12);

        if (packet.Data is not null)
        {
            writer.Write(packet.Data);
        }
    }

    public static RTCPApplicationDefinedPacket Deserialize(ReadOnlySpan<byte> packet, out int consumed)
    {
        Debug.Assert(packet[1] == (byte)RTCPPacketType.ApplicationDefined);
        ArgumentOutOfRangeException.ThrowIfNotEqual(packet[0] & 0b11000000, 0b10000000, "RTCP packet version");

        ushort length = BinaryPrimitives.ReadUInt16BigEndian(packet[2..]);
        int subtype = packet[0] & 0b00011111;

        uint ssrc = BinaryPrimitives.ReadUInt32BigEndian(packet[4..]);
        string name = Encoding.ASCII.GetString(packet[8..12]);

        byte[] data = new byte[(length - 2) * 4];
        packet[12..].CopyTo(data);

        consumed = (length + 1) * 4;

        return new()
        {
            Subtype = subtype,
            SSRC = ssrc,
            Name = name,
            Data = data
        };
    }
}
