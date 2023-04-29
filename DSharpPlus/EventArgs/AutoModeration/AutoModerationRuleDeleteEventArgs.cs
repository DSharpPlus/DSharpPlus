using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

public class AutoModerationRuleDeleteEventArgs : DiscordEventArgs
{
    public DiscordAutoModerationRule Rule { get; internal set; }

    internal AutoModerationRuleDeleteEventArgs() : base()
    {

    }

    internal AutoModerationRuleDeleteEventArgs(DiscordAutoModerationRule rule) : base()
    {
        this.Rule = rule;
    }
}
