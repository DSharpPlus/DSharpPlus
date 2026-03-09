using System;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Protocol.RTCP.Payloads;
using DSharpPlus.Voice.Protocol.RTCP.Serialization;

namespace DSharpPlus.Voice.Protocol.RTCP;

/// <summary>
/// Provides a mechanism to serialize and deserialize RTCP packets.
/// </summary>
internal static class RTCPSerializer
{
    public static void Serialize(IRTCPPacket packet, ArrayPoolBufferWriter<byte> writer)
    {
        switch (packet.Type)
        {
            case RTCPPacketType.SenderReport:
                SenderReportSerializer.Serialize((RTCPSenderReportPacket)packet, writer);
                break;

            case RTCPPacketType.ReceiverReport:
                ReceiverReportSerializer.Serialize((RTCPReceiverReportPacket)packet, writer);
                break;

            case RTCPPacketType.SourceDescription:
                SourceDescriptionSerializer.Serialize((RTCPSourceDescriptionPacket)packet, writer);
                break;

            case RTCPPacketType.Goodbye:
                GoodbyeSerializer.Serialize((RTCPGoodbyePacket)packet, writer);
                break;

            case RTCPPacketType.ApplicationDefined:
                ApplicationDefinedSerializer.Serialize((RTCPApplicationDefinedPacket)packet, writer);
                break;

            default:
                throw new NotImplementedException($"The RTCP packet type {packet.Type} is not supported.");
        }
    }
}
