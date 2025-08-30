using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Transport.Models.VoicePayloads.Inbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.Hello"/>.
/// </summary>
internal sealed record VoiceHelloPayload
{
    /// <summary>
    /// The time in milliseconds between heartbeats sent to the server.
    /// </summary>
    [JsonPropertyName("heartbeat_interval")]
    public required uint HeartbeatInterval { get; init; }
}
