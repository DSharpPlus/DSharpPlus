using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for GuildIntegrationsUpdated event.
/// </summary>
public class GuildIntegrationsUpdatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild that had its integrations updated.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal GuildIntegrationsUpdatedEventArgs() : base() { }
}
