using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents argument for <see cref="DiscordClient.AutoModerationRuleCreated"/> event.
/// </summary>
public class AutoModerationRuleCreateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the created rule.
    /// </summary>
    public DiscordAutoModerationRule? Rule { get; internal set; }

    internal AutoModerationRuleCreateEventArgs() : base() { }

    internal AutoModerationRuleCreateEventArgs(DiscordAutoModerationRule rule) : base() => Rule = rule;
}
