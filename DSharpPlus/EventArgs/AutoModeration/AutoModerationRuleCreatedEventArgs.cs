using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents argument for <see cref="DiscordClient.AutoModerationRuleCreated"/> event.
/// </summary>
public class AutoModerationRuleCreatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the created rule.
    /// </summary>
    public DiscordAutoModerationRule? Rule { get; internal set; }

    internal AutoModerationRuleCreatedEventArgs() : base() { }

    internal AutoModerationRuleCreatedEventArgs(DiscordAutoModerationRule rule) : base() => this.Rule = rule;
}
