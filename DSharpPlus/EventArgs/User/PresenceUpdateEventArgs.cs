using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

using Caching;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.PresenceUpdated"/> event.
/// </summary>
public class PresenceUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the user whose presence was updated.
    /// </summary>
    public CachedEntity<ulong, DiscordUser> User { get; internal set; }

    /// <summary>
    /// Gets the user's new game.
    /// </summary>
    public DiscordActivity Activity { get; internal set; }

    /// <summary>
    /// Gets the user's status.
    /// </summary>
    public UserStatus Status { get; internal set; }

    /// <summary>
    /// Gets the user's old presence. This is null if the user was not cached prior to presence update.
    /// </summary>
    public DiscordPresence? PresenceBefore { get; internal set; }

    /// <summary>
    /// Gets the user's new presence.
    /// </summary>
    public DiscordPresence PresenceAfter { get; internal set; }

    /// <summary>
    /// Gets the user prior to presence update.
    /// </summary>
    public DiscordUser UserBefore { get; internal set; }

    /// <summary>
    /// Gets the user after the presence update.
    /// </summary>
    public DiscordUser UserAfter { get; internal set; }

    internal PresenceUpdateEventArgs() : base() { }
}
