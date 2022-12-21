using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.VoiceGateway.Payloads;

/// <summary>
/// In order to maintain your WebSocket connection, you need to continuously send heartbeats at the interval determined in <see cref="Enums.InternalVoiceOpCode.Hello"/>.
/// </summary>
public sealed record InternalVoiceHelloPayload
{
    /// <summary>
    /// Time to wait between sending heartbeats in milliseconds.
    /// </summary>
    [JsonPropertyName("heartbeat_interval")]
    public int HeartbeatInterval { get; init; }
}
