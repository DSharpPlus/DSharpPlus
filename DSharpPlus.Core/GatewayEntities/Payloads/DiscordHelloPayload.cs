using DSharpPlus.Core.Attributes;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Payloads
{
    /// <summary>
    /// Sent on connection to the websocket. Defines the heartbeat interval that the client should heartbeat to.
    /// </summary>
    [DiscordGatewayPayload("HELLO")]
    public sealed record DiscordHelloPayload
    {
        /// <summary>
        /// The interval (in milliseconds) the client should heartbeat with.
        /// </summary>
        [JsonProperty("heartbeat_interval", NullValueHandling = NullValueHandling.Ignore)]
        public int HeartbeatInterval { get; init; }
    }
}
