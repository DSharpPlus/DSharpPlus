using System.Text.Json.Serialization;

using DSharpPlus.Voice.Protocol.Gateway.DaveV1;

namespace DSharpPlus.Voice.Protocol.Gateway.DaveV1.Inbound;

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
