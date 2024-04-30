namespace DSharpPlus.EventArgs;
using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.AutoModerationRuleDeleted"/> event.
/// </summary>
public class AutoModerationRuleDeleteEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the deleted rule.
    /// </summary>
    public DiscordAutoModerationRule Rule { get; internal set; }

    internal AutoModerationRuleDeleteEventArgs() : base() { }

    internal AutoModerationRuleDeleteEventArgs(DiscordAutoModerationRule rule) : base() => Rule = rule;
}
