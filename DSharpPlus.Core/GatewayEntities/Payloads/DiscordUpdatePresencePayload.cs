using System;
using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Enums;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Payloads
{
    /// <summary>
    /// A user's presence is their current state on a guild. This event is sent when a user's presence or info, such as name or avatar, is updated.
    /// </summary>
    [DiscordGatewayPayload("PRESENCE_UPDATE")]
    public sealed record DiscordUpdatePresencePayload
    {
        /// <summary>
        /// The user presence is being updated for.
        /// </summary>
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser User { get; init; } = null!;

        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// Either "idle", "dnd", "online", or "offline".
        /// </summary>
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordStatusType Status { get; init; }

        /// <summary>
        /// The user's current activities.
        /// </summary>
        [JsonProperty("activities", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordActivity> Activities { get; init; } = Array.Empty<DiscordActivity>();

        /// <summary>
        /// The user's platform-dependent status.
        /// </summary>
        [JsonProperty("client_status", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordClientStatus ClientStatus { get; init; } = null!;
    }
}
