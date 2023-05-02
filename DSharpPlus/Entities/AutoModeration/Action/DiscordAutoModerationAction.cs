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

    public DiscordAutoModerationAction()
    {

    }
}
