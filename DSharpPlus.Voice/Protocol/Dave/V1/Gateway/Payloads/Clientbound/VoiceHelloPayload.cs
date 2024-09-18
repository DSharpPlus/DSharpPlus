using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Clientbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.Hello"/>.
/// </summary>
internal sealed record VoiceHelloPayload : IVoicePayload
{
    /// <summary>
    /// The time in milliseconds between heartbeats sent to the server.
    /// </summary>
    [JsonPropertyName("heartbeat_interval")]
    public required uint HeartbeatInterval { get; init; }
}
