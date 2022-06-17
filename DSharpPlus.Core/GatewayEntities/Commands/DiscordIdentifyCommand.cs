using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using DSharpPlus.Core.RestEntities;
using System.Text.Json.Serialization;

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
        [JsonPropertyName("token")]
        public string Token { get; init; } = null!;

        /// <summary>
        /// The connection properties.
        /// </summary>
        [JsonPropertyName("properties")]
        public DiscordIdentifyConnectionProperties Properties { get; init; } = null!;

        /// <summary>
        /// Whether this connection supports compression of packets.
        /// </summary>
        [JsonPropertyName("compress")]
        public Optional<bool> Compress { get; init; } = true;

        /// <summary>
        /// A value between 50 and 250, total number of members where the gateway will stop sending offline members in the guild member list.
        /// </summary>
        [JsonPropertyName("large_threshold")]
        public Optional<int> LargeThreshold { get; init; } = 50;

        /// <summary>
        /// Used for Guild Sharding.
        /// </summary>
        [JsonPropertyName("shard")]
        public Optional<Dictionary<int, int>> Shard { get; init; }

        /// <summary>
        /// The presence structure for initial presence information.
        /// </summary>
        [JsonPropertyName("presence")]
        public Optional<DiscordPresenceUpdateCommand> Presence { get; init; }

        /// <summary>
        /// The <see cref="DiscordGatewayIntents"/> you wish to receive.
        /// </summary>
        [JsonPropertyName("intents")]
        public DiscordGatewayIntents Intents { get; init; }
    }
}
