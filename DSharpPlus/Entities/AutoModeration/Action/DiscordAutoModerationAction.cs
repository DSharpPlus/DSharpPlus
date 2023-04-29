using DSharpPlus.Enums;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public class DiscordAutoModerationAction
{
    [JsonProperty("type")]
    public RuleActionType? Type { get; internal set; }

    /// <summary>
    /// Gets additional metadata needed during execution for this specific action type.
    /// </summary>
    [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
    public RuleActionMetadata? Metadata { get; internal set; }

    public DiscordAutoModerationAction()
    {

    }

    internal DiscordAutoModerationAction(RuleActionType? ruleActionType = null, RuleActionMetadata? ruleActionMetadata = null)
    {
        this.Type = ruleActionType;
        this.Metadata = ruleActionMetadata;
    }
}


public class DiscordAutoModerationActionBuilder
{
    public RuleActionType? Type { internal get; set; }

    public RuleActionMetadata? Metadata { internal get; set; }

    public DiscordAutoModerationActionBuilder()
    {

    }

    public DiscordAutoModerationActionBuilder(RuleActionType? ruleActionType = null, RuleActionMetadata? ruleActionMetadata = null)
    {
        this.Type = ruleActionType;
        this.Metadata = ruleActionMetadata;
    }

    public DiscordAutoModerationAction Build() => new DiscordAutoModerationAction(this.Type, this.Metadata);
}
