using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildIntegrationsUpdated"/> event.
/// </summary>
public class GuildIntegrationsUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild that had its integrations updated. This field is null if the guild was not in cache.
    /// </summary>
    public DiscordGuild? Guild { get; internal set; }
    
    /// <summary>
    /// Gets the id of the guild that had its integrations updated.
    /// </summary>
    public ulong GuildId { get; internal set; }

    internal GuildIntegrationsUpdateEventArgs() : base() { }
}
