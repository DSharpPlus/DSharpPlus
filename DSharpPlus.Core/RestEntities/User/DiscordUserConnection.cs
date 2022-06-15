using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordUserConnection
    {
        /// <summary>
        /// The id of the connection account.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; init; } = null!;

        /// <summary>
        /// The username of the connection account.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The service of the connection (twitch, youtube).
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; init; } = null!;

        /// <summary>
        /// Whether the connection is revoked.
        /// </summary>
        [JsonProperty("revoked", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Revoked { get; init; }

        /// <summary>
        /// An array of partial server integrations.
        /// </summary>
        [JsonProperty("integrations", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyList<DiscordIntegration>> Integrations { get; init; }

        /// <summary>
        /// Whether the connection is verified.
        /// </summary>
        [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
        public bool Verified { get; init; }

        /// <summary>
        /// Whether friend sync is enabled for this connection.
        /// </summary>
        [JsonProperty("friend_sync", NullValueHandling = NullValueHandling.Ignore)]
        public bool FriendSync { get; init; }

        /// <summary>
        /// Whether activities related to this connection will be shown in presence updates.
        /// </summary>
        [JsonProperty("show_activity", NullValueHandling = NullValueHandling.Ignore)]
        public bool ShowActivity { get; init; }

        /// <summary>
        /// The visibility of this connection.
        /// </summary>
        [JsonProperty("visibility", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUserConnectionVisibilityType Visibility { get; init; }
    }
}
