namespace DSharpPlus.Voice.Protocol.RTCP;

// RFC 3550 Section 12.1

/// <summary>
/// Specifies the type of a RTCP packet.
/// </summary>
internal enum RTCPPacketType
{
    SenderReport = 200,
    ReceiverReport,
    SourceDescription,
    Goodbye,
    ApplicationDefined
}
