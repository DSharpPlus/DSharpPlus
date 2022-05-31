using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Commands
{
    /// <summary>
    /// Used to trigger the initial handshake with the gateway.
    /// </summary>
    public sealed record DiscordIdentifyCommand
    {
        /// <summary>
        /// The authentication token.
        /// </summary>
        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; init; } = null!;

        /// <summary>
        /// The connection properties.
        /// </summary>
        [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordIdentifyConnectionProperties Properties { get; init; } = null!;

        /// <summary>
        /// Whether this connection supports compression of packets.
        /// </summary>
        [JsonProperty("compress", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Compress { get; init; } = true;

        /// <summary>
        /// A value between 50 and 250, total number of members where the gateway will stop sending offline members in the guild member list.
        /// </summary>
        [JsonProperty("large_threshold", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> LargeThreshold { get; init; } = 50;

        /// <summary>
        /// Used for Guild Sharding.
        /// </summary>
        [JsonProperty("shard", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<Dictionary<int, int>> Shard { get; init; }

        /// <summary>
        /// The presence structure for initial presence information.
        /// </summary>
        [JsonProperty("presence", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordPresenceUpdateCommand> Presence { get; init; }

        /// <summary>
        /// The <see cref="DiscordGatewayIntents"/> you wish to receive.
        /// </summary>
        [JsonProperty("intents", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGatewayIntents Intents { get; init; }
    }
}
