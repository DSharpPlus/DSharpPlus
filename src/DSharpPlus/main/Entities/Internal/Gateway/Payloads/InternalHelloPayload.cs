using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent on connection to the websocket. Defines the heartbeat interval that the client should heartbeat to.
/// </summary>
public sealed record InternalHelloPayload
{
    /// <summary>
    /// The interval (in milliseconds) the client should heartbeat with.
    /// </summary>
    [JsonPropertyName("heartbeat_interval")]
    public int HeartbeatInterval { get; init; }
}
