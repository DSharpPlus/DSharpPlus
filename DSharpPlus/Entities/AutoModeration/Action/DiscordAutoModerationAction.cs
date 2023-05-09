using DSharpPlus.Enums;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Discord auto moderation action.
/// </summary>
public class DiscordAutoModerationAction
{
    /// <summary>
    /// Gets the rule action type.
    /// </summary>
    [JsonProperty("type")]
    public RuleActionType Type { get; internal set; }

    /// <summary>
    /// Gets additional metadata needed during execution for this specific action type.
    /// </summary>
    [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordRuleActionMetadata? Metadata { get; internal set; }
}

/// <summary>
/// Constructs auto-moderation actions.
/// </summary>
public class DiscordAutoModerationActionBuilder
{
    /// <summary>
    /// Sets the rule action type.
    /// </summary>
    public RuleActionType Type { get; internal set; }

    /// <summary>
    /// Sets additional metadata needed during execution for this specific action type.
    /// </summary>
    public DiscordRuleActionMetadata? Metadata { get; internal set; }

    public DiscordAutoModerationActionBuilder WithRuleActionType(RuleActionType type)
    {
        this.Type = type;

        return this;
    }

    public DiscordAutoModerationActionBuilder WithActionMetadata(DiscordRuleActionMetadata metadata)
    {
        this.Metadata = metadata;

        return this;
    }

    public DiscordAutoModerationAction Build() => new DiscordAutoModerationAction
    {
        Type = this.Type,
        Metadata = this.Metadata
    };
}
