using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Serverbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.Heartbeat"/>
/// </summary>
internal sealed record VoiceHeartbeatPayload : IVoicePayload
{
    /// <summary>
    /// An integer nonce constructed using the current timestamp.
    /// </summary>
    [JsonPropertyName("t")]
    public required long Nonce { get; init; }

    /// <summary>
    /// The last received sequence number received from the gateway.
    /// </summary>
    [JsonPropertyName("seq_ack")]
    public required int LastAcknowledgedSequence { get; init; }
}
