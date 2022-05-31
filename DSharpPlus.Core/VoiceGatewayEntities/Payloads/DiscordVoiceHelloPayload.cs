using Newtonsoft.Json;

namespace DSharpPlus.Core.VoiceGatewayEntities.Payloads
{
    /// <summary>
    /// In order to maintain your WebSocket connection, you need to continuously send heartbeats at the interval determined in <see cref="Enums.DiscordVoiceOpCode.Hello"/>.
    /// </summary>
    public sealed record DiscordVoiceHelloPayload
    {
        /// <summary>
        /// Time to wait between sending heartbeats in milliseconds.
        /// </summary>
        [JsonProperty("heartbeat_interval", NullValueHandling = NullValueHandling.Ignore)]
        public int HeartbeatInterval { get; init; }
    }
}
