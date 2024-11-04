using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for GuildUpdated event.
/// </summary>
public class GuildUpdatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild before it was updated.
    /// </summary>
    public DiscordGuild GuildBefore { get; internal set; }

    /// <summary>
    /// Gets the guild after it was updated.
    /// </summary>
    public DiscordGuild GuildAfter { get; internal set; }

    internal GuildUpdatedEventArgs() : base() { }
}
