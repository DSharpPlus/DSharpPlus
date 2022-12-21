using DSharpPlus.Core.Attributes;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent on connection to the websocket. Defines the heartbeat interval that the client should heartbeat to.
    /// </summary>
    [InternalGatewayPayload("HELLO")]
    public sealed record InternalHelloPayload
    {
        /// <summary>
        /// The interval (in milliseconds) the client should heartbeat with.
        /// </summary>
        [JsonPropertyName("heartbeat_interval")]
        public int HeartbeatInterval { get; init; }
    }
}
