using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Protocol.RTCP.Payloads;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice.Protocol.RTCP;

/// <summary>
/// Provides a mechanism to serialize and deserialize RTCP packets.
/// </summary>
internal static partial class RTCPSerializer
{
    private const byte VersionMask = 0b1100_0000;
    private const byte RTPVersion = 0b1000_0000;

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

    public static IReadOnlyList<IRTCPPacket> Deserialize(ReadOnlySpan<byte> buffer, ILogger logger)
    {
        List<IRTCPPacket> packets = [];

        // 4 is the absolute minimum length of a valid RTCP packet, even if it's not meaningful
        while (buffer.Length >= 4)
        {
            if (!IsValidRTCPPacketVersion(buffer[0]))
            {
                break;
            }

            ushort specifiedLength = BinaryPrimitives.ReadUInt16BigEndian(buffer[2..]);
            int effectiveLength = (specifiedLength + 1) * 4;

            if (effectiveLength > buffer.Length)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(
                        "Malformed RTCP packet: packet length set to {specifiedLength}, which is larger than buffer length {bufferLength}.",
                        effectiveLength, buffer.Length
                    );
                }
                break;
            }

            ReadOnlySpan<byte> packet = buffer[..effectiveLength];
            int consumed = effectiveLength;

            IRTCPPacket? deserialized = (RTCPPacketType)packet[1] switch
            {
                RTCPPacketType.SenderReport => DeserializeSenderReport(packet, out consumed),
                RTCPPacketType.ReceiverReport => DeserializeReceiverReport(packet, out consumed),
                RTCPPacketType.SourceDescription => DeserializeSourceDescription(packet, out consumed),
                RTCPPacketType.Goodbye => DeserializeGoodbye(packet, out consumed),
                RTCPPacketType.ApplicationDefined => DeserializeApplicationDefinedPacket(packet, out consumed),
                _ => null,
                    
            };

            if (deserialized is not null)
            {
                Debug.Assert(consumed == effectiveLength);
                packets.Add(deserialized);
            }
            else if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("The RTCP packet type {RtcpPacketType} is not supported.", (RTCPPacketType)packet[1]);
            }

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
        if (!IsValidRTCPPacketVersion(buffer[0]))
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

    private static bool IsValidRTCPPacketVersion(byte versionByte)
        => (versionByte & VersionMask) == RTPVersion;
}
