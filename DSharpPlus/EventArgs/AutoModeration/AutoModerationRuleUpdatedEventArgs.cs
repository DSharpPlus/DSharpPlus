using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.AutoModerationRuleUpdated"/> event.
/// </summary>
public class AutoModerationRuleUpdatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the updated rule.
    /// </summary>
    public DiscordAutoModerationRule? Rule { get; internal set; }

    internal AutoModerationRuleUpdatedEventArgs() : base() { }

    internal AutoModerationRuleUpdatedEventArgs(DiscordAutoModerationRule rule) : base() => this.Rule = rule;
}
