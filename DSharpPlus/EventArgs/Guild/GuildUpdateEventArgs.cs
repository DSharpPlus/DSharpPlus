using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildUpdated"/> event.
/// </summary>
public class GuildUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild before it was updated. This property might be null if the guild wasn't cached.
    /// </summary>
    public DiscordGuild? GuildBefore { get; internal set; }

    /// <summary>
    /// Gets the guild after it was updated.
    /// </summary>
    public DiscordGuild GuildAfter { get; internal set; }

    internal GuildUpdateEventArgs() : base() { }
}
