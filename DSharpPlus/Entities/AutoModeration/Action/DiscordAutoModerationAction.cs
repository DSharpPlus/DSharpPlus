using DSharpPlus.Enums;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public class DiscordAutoModerationAction
{
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
