using System.Text.Json.Serialization;

using DSharpPlus.Voice.Protocol.Gateway.Payloads.Serverbound;

namespace DSharpPlus.Voice.Protocol.Gateway.Payloads.Clientbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.HeartbeatAck"/>
/// </summary>
internal sealed record VoiceHeartbeatAcknowledgedPayload : IVoicePayload
{
    /// <summary>
    /// An integer nonce matching the nonce sent in the matching <see cref="VoiceHeartbeatPayload"/>.
    /// </summary>
    [JsonPropertyName("t")]
    public required long Nonce { get; init; }
}
