using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalAutoModerationRule
{
    public static readonly ReadOnlyDictionary<DiscordAutoModerationTriggerType, int> TriggerTypeLimits = 
        new(new Dictionary<DiscordAutoModerationTriggerType, int>()
    {
        [DiscordAutoModerationTriggerType.Keyword] = 3,
        [DiscordAutoModerationTriggerType.HarmfulLink] = 1,
        [DiscordAutoModerationTriggerType.Spam] = 1,
        [DiscordAutoModerationTriggerType.KeywordPreset] = 1 
    });

    /// <summary>
    /// The id of this rule.
    /// </summary>
    [JsonPropertyName("id")]
    public required Snowflake Id { get; init; }

    /// <summary>
    /// The guild which this rule belongs to.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public required Snowflake GuildId { get; init; }

    /// <summary>
    /// The rule name.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; } 

    /// <summary>
    /// The user which first created this rule.
    /// </summary>
    [JsonPropertyName("creator_id")]
    public required Snowflake CreatorId { get; init; } 

    /// <summary>
    /// The rule event type.
    /// </summary>
    [JsonPropertyName("event_type")]
    public required DiscordAutoModerationEventType EventType { get; init; }

    /// <summary>
    /// The rule trigger type.
    /// </summary>
    [JsonPropertyName("trigger_type")]
    public required DiscordAutoModerationTriggerType TriggerType { get; init; }

    /// <summary>
    /// The rule trigger metadata.
    /// </summary>
    [JsonPropertyName("trigger_metadata")]
    public required InternalAutoModerationTriggerMetadata TriggerMetadata { get; init; } 

    /// <summary>
    /// The actions which will execute when the rule is triggered.
    /// </summary>
    [JsonPropertyName("actions")]
    public required IReadOnlyList<InternalAutoModerationAction> Action { get; init; }

    /// <summary>
    /// Whether the rule is enabled.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }

    /// <summary>
    /// The role ids that should not be affected by the rule (Maximum of 20).
    /// </summary>
    [JsonPropertyName("exempt_roles")]
    public required IReadOnlyList<Snowflake> ExemptRoles { get; init; }

    /// <summary>
    /// The channel ids that should not be affected by the rule (Maximum of 50).
    /// </summary>
    [JsonPropertyName("exempt_channels")]
    public required IReadOnlyList<Snowflake> ExemptChannels { get; init; }
}
