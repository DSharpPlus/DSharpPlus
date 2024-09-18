using DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway;

internal class VoiceGatewayEvent
{
    public required VoiceGatewayOpcode Opcode { get; init; }

    public int Sequence { get; init; }

    public IVoicePayload Payload { get; init; }
}
