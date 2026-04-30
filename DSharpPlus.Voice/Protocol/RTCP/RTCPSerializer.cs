using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Protocol.RTCP.Payloads;

namespace DSharpPlus.Voice.Protocol.RTCP;

/// <summary>
/// Provides a mechanism to serialize and deserialize RTCP packets.
/// </summary>
internal static partial class RTCPSerializer
{
    public static void Serialize(IRTCPPacket packet, ArrayPoolBufferWriter<byte> writer)
    {
        switch (packet.Type)
        {
            case RTCPPacketType.SenderReport:
                SerializeSenderReport((RTCPSenderReportPacket)packet, writer);
                break;

            case RTCPPacketType.ReceiverReport:
                SerializeReceiverReport((RTCPReceiverReportPacket)packet, writer);
                break;

            case RTCPPacketType.SourceDescription:
                SerializeSourceDescription((RTCPSourceDescriptionPacket)packet, writer);
                break;

            case RTCPPacketType.Goodbye:
                SerializeGoodbye((RTCPGoodbyePacket)packet, writer);
                break;

            case RTCPPacketType.ApplicationDefined:
                SerializeApplicationDefinedPacket((RTCPApplicationDefinedPacket)packet, writer);
                break;

            default:
                throw new NotImplementedException($"The RTCP packet type {packet.Type} is not supported.");
        }
    }

    public static IReadOnlyList<IRTCPPacket> Deserialize(ReadOnlySpan<byte> buffer)
    {
        List<IRTCPPacket> packets = [];

        // 4 is the absolute minimum length of a valid RTCP packet, even if it's not meaningful
        while (buffer.Length >= 4)
        {
            ushort specifiedLength = BinaryPrimitives.ReadUInt16BigEndian(buffer[2..]);
            int effectiveLength = (specifiedLength + 1) * 4;

            ReadOnlySpan<byte> packet = buffer[..effectiveLength];
            int consumed;

            IRTCPPacket deserialized = (RTCPPacketType)packet[1] switch
            {
                RTCPPacketType.SenderReport => DeserializeSenderReport(packet, out consumed),
                RTCPPacketType.ReceiverReport => DeserializeReceiverReport(packet, out consumed),
                RTCPPacketType.SourceDescription => DeserializeSourceDescription(packet, out consumed),
                RTCPPacketType.Goodbye => DeserializeGoodbye(packet, out consumed),
                RTCPPacketType.ApplicationDefined => DeserializeApplicationDefinedPacket(packet, out consumed),
                _ => throw new NotImplementedException($"The RTCP packet type {(RTCPPacketType)packet[1]} is not supported.")
            };

            Debug.Assert(consumed == effectiveLength);
            packets.Add(deserialized);

            buffer = buffer[effectiveLength..];
        }

        return packets;
    }

    public static bool IsValidRTCPPacket(ReadOnlySpan<byte> buffer)
    {
        // not length-aligned or long enough
        if (buffer.Length % 4 != 0 || buffer.Length < 4)
        {
            return false;
        }

        // invalid version
        if ((buffer[0] & 0b1100_0000) != 0b1000_0000)
        {
            return false;
        }

        // invalid packet type
        if (buffer[1] is < 200 or > 204)
        {
            return false;
        }

        return true;
    }
}
