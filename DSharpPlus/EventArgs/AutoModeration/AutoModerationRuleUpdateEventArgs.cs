namespace DSharpPlus.EventArgs;

using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.AutoModerationRuleUpdated"/> event.
/// </summary>
public class AutoModerationRuleUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the updated rule.
    /// </summary>
    public DiscordAutoModerationRule? Rule { get; internal set; }

    internal AutoModerationRuleUpdateEventArgs() : base() { }

    internal AutoModerationRuleUpdateEventArgs(DiscordAutoModerationRule rule) : base() => Rule = rule;
}
