using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for the AutoModerationRuleExecuted event.
/// </summary>
public class AutoModerationRuleExecutedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the executed rule.
    /// </summary>
    public DiscordAutoModerationActionExecution Rule { get; internal set; }

    internal AutoModerationRuleExecutedEventArgs() : base() { }

    internal AutoModerationRuleExecutedEventArgs(DiscordAutoModerationActionExecution rule) : base() => this.Rule = rule;
}
