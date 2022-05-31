using System;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// A Stage Instance holds information about a live stage.
    /// </summary>
    [DiscordGatewayPayload("STAGE_INSTANCE_CREATE", "STAGE_INSTANCE_UPDATE", "STAGE_INSTANCE_DELETE")]
    public sealed record DiscordStageInstance
    {
        /// <summary>
        /// The id of this Stage instance.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The guild id of the associated Stage channel.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The id of the associated Stage channel.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The topic of the Stage instance (1-120 characters).
        /// </summary>
        [JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
        public string Topic { get; init; } = null!;

        /// <summary>
        /// The privacy level of the Stage instance.
        /// </summary>
        [JsonProperty("privacy", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordStageInstancePrivacyLevel PrivacyLevel { get; init; }

        /// <summary>
        /// Whether or not Stage Discovery is disabled (deprecated).
        /// </summary>
        [JsonProperty("discovery_disabled", NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete("Whether or not Stage Discovery is disabled (deprecated)")]
        public bool DiscoverableDisabled { get; set; }

        /// <summary>
        /// The id of the scheduled event for this Stage instance.
        /// </summary>
        [JsonProperty("guild_scheduled_event_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake? GuildScheduledEventId { get; init; }
    }
}
