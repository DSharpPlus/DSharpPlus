using System.Text.Json.Serialization;

using DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Serverbound;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Clientbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.HeartbeatAck"/>
/// </summary>
internal sealed record VoiceHeartbeatAcknowledgedPayload
{
    /// <summary>
    /// An integer nonce matching the nonce sent in the matching <see cref="VoiceHeartbeatPayload"/>.
    /// </summary>
    [JsonPropertyName("t")]
    public required long Nonce { get; init; }
}
