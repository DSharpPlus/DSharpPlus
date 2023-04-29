using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

public class AutoModerationRuleCreateEventArgs : DiscordEventArgs
{
    public DiscordAutoModerationRule? Rule { get; internal set; }

    internal AutoModerationRuleCreateEventArgs() : base()
    {

    }

    internal AutoModerationRuleCreateEventArgs(DiscordAutoModerationRule rule) : base()
    {
        this.Rule = rule;
    }
}
