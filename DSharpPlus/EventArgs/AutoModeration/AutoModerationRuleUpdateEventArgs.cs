using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

public class AutoModerationRuleUpdateEventArgs : DiscordEventArgs
{
    public DiscordAutoModerationRule? Rule { get; internal set; }

    internal AutoModerationRuleUpdateEventArgs() : base()
    {

    }

    internal AutoModerationRuleUpdateEventArgs(DiscordAutoModerationRule rule) : base()
    {
        this.Rule = rule;
    }
}
