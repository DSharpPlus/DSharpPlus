namespace DSharpPlus.EventArgs;
using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.AutoModerationRuleExecuted"/> event.
/// </summary>
public class AutoModerationRuleExecuteEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the executed rule.
    /// </summary>
    public DiscordAutoModerationActionExecution Rule { get; internal set; }

    internal AutoModerationRuleExecuteEventArgs() : base() { }

    internal AutoModerationRuleExecuteEventArgs(DiscordAutoModerationActionExecution rule) : base() => Rule = rule;
}
