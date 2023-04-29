using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

public class AutoModerationRuleExecuteEventArgs : DiscordEventArgs
{
    public AutoModerationActionExecution Rule { get; internal set; }

    internal AutoModerationRuleExecuteEventArgs() : base()
    {

    }

    internal AutoModerationRuleExecuteEventArgs(AutoModerationActionExecution rule) : base()
    {
        this.Rule = rule;
    }
}
