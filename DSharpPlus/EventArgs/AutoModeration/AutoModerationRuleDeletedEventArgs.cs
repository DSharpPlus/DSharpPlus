using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for the AutoModerationRuleDeleted event.
/// </summary>
public class AutoModerationRuleDeletedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the deleted rule.
    /// </summary>
    public DiscordAutoModerationRule Rule { get; internal set; }

    internal AutoModerationRuleDeletedEventArgs() : base() { }

    internal AutoModerationRuleDeletedEventArgs(DiscordAutoModerationRule rule) : base() => this.Rule = rule;
}
