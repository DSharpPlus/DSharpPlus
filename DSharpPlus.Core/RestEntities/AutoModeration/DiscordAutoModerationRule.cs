using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

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
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The guild which this rule belongs to.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The rule name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The user which first created this rule.
        /// </summary>
        [JsonPropertyName("creator_id")]
        public DiscordSnowflake CreatorId { get; init; } = null!;

        /// <summary>
        /// The rule event type.
        /// </summary>
        [JsonPropertyName("event_type")]
        public DiscordAutoModerationEventType EventType { get; init; }

        /// <summary>
        /// The rule trigger type.
        /// </summary>
        [JsonPropertyName("trigger_type")]
        public DiscordAutoModerationTriggerType TriggerType { get; init; }

        /// <summary>
        /// The rule trigger metadata.
        /// </summary>
        [JsonPropertyName("trigger_metadata")]
        public DiscordAutoModerationTriggerMetadata TriggerMetadata { get; init; } = null!;

        /// <summary>
        /// The actions which will execute when the rule is triggered.
        /// </summary>
        [JsonPropertyName("actions")]
        public IReadOnlyList<DiscordAutoModerationAction> Action { get; init; } = Array.Empty<DiscordAutoModerationAction>();

        /// <summary>
        /// Whether the rule is enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; init; }

        /// <summary>
        /// The role ids that should not be affected by the rule (Maximum of 20).
        /// </summary>
        [JsonPropertyName("exempt_roles")]
        public IReadOnlyList<DiscordSnowflake> ExemptRoles { get; init; } = Array.Empty<DiscordSnowflake>();

        /// <summary>
        /// The channel ids that should not be affected by the rule (Maximum of 50).
        /// </summary>
        [JsonPropertyName("exempt_channels")]
        public IReadOnlyList<DiscordSnowflake> ExemptChannels { get; init; } = Array.Empty<DiscordSnowflake>();
    }
}
