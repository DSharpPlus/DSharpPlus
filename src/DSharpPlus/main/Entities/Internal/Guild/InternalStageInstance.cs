using System;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// A Stage Instance holds information about a live stage.
    /// </summary>
    [InternalGatewayPayload("STAGE_INSTANCE_CREATE", "STAGE_INSTANCE_UPDATE", "STAGE_INSTANCE_DELETE")]
    public sealed record InternalStageInstance
    {
        /// <summary>
        /// The id of this Stage instance.
        /// </summary>
        [JsonPropertyName("id")]
        public InternalSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The guild id of the associated Stage channel.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public InternalSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The id of the associated Stage channel.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public InternalSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The topic of the Stage instance (1-120 characters).
        /// </summary>
        [JsonPropertyName("topic")]
        public string Topic { get; init; } = null!;

        /// <summary>
        /// The privacy level of the Stage instance.
        /// </summary>
        [JsonPropertyName("privacy")]
        public InternalStageInstancePrivacyLevel PrivacyLevel { get; init; }

        /// <summary>
        /// Whether or not Stage Discovery is disabled (deprecated).
        /// </summary>
        [JsonPropertyName("discovery_disabled")]
        [Obsolete("Whether or not Stage Discovery is disabled (deprecated)")]
        public bool DiscoverableDisabled { get; set; }

        /// <summary>
        /// The id of the scheduled event for this Stage instance.
        /// </summary>
        [JsonPropertyName("guild_scheduled_event_id")]
        public InternalSnowflake? GuildScheduledEventId { get; init; }
    }
}
