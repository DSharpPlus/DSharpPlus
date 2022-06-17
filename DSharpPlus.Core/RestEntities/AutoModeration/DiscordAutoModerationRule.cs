using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    [DiscordGatewayPayload("AUTO_MODERATION_RULE_CREATE", "AUTO_MODERATION_RULE_UPDATE", "AUTO_MODERATION_RULE_DELETE")]
    public sealed record DiscordAutoModerationRule
    {
        public static readonly ReadOnlyDictionary<DiscordAutoModerationTriggerType, int> TriggerTypeLimits = new(new Dictionary<DiscordAutoModerationTriggerType, int>()
        {
            { DiscordAutoModerationTriggerType.Keyword, 3 },
            { DiscordAutoModerationTriggerType.HarmfulLink, 1 },
            { DiscordAutoModerationTriggerType.Spam, 1 },
            { DiscordAutoModerationTriggerType.KeywordPreset, 1 }
        });

        /// <summary>
        /// The id of this rule.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The guild which this rule belongs to.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The rule name.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The user which first created this rule.
        /// </summary>
        [JsonProperty("creator_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake CreatorId { get; init; } = null!;

        /// <summary>
        /// The rule event type.
        /// </summary>
        [JsonProperty("event_type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordAutoModerationEventType EventType { get; init; }

        /// <summary>
        /// The rule trigger type.
        /// </summary>
        [JsonProperty("trigger_type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordAutoModerationTriggerType TriggerType { get; init; }

        /// <summary>
        /// The rule trigger metadata.
        /// </summary>
        [JsonProperty("trigger_metadata", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordAutoModerationTriggerMetadata TriggerMetadata { get; init; } = null!;

        /// <summary>
        /// The actions which will execute when the rule is triggered.
        /// </summary>
        [JsonProperty("actions", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordAutoModerationAction> Action { get; init; } = Array.Empty<DiscordAutoModerationAction>();

        /// <summary>
        /// Whether the rule is enabled.
        /// </summary>
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool Enabled { get; init; }

        /// <summary>
        /// The role ids that should not be affected by the rule (Maximum of 20).
        /// </summary>
        [JsonProperty("exempt_roles", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordSnowflake> ExemptRoles { get; init; } = Array.Empty<DiscordSnowflake>();

        /// <summary>
        /// The channel ids that should not be affected by the rule (Maximum of 50).
        /// </summary>
        [JsonProperty("exempt_channels", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordSnowflake> ExemptChannels { get; init; } = Array.Empty<DiscordSnowflake>();
    }
}
