using System.Text.Json.Serialization;

using DSharpPlus.Voice.Protocol.Gateway.Payloads;

namespace DSharpPlus.Voice.Protocol.Gateway;

/// <summary>
/// Represents a message sent or received from the voice gateway.
/// </summary>
[JsonConverter(typeof(VoiceGatewayMessageConverter))]
public sealed record VoiceGatewayMessage
{
    /// <summary>
    /// The opcode of this message.
    /// </summary>
    public required VoiceGatewayOpcode Opcode { get; init; }

    /// <summary>
    /// If this is a message received from Discord, the sequence number of that message.
    /// </summary>
    public int Sequence { get; internal init; }

    /// <summary>
    /// The payload enclosed within this message, typed according to <see cref="Opcode"/>.
    /// </summary>
    public IVoicePayload Payload { get; init; }
}
