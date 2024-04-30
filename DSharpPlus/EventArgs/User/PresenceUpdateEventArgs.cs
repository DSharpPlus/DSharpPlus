namespace DSharpPlus.EventArgs;
using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.PresenceUpdated"/> event.
/// </summary>
public class PresenceUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the user whose presence was updated.
    /// </summary>
    public DiscordUser User { get; internal set; }

    /// <summary>
    /// Gets the user's new game.
    /// </summary>
    public DiscordActivity Activity { get; internal set; }

    /// <summary>
    /// Gets the user's status.
    /// </summary>
    public DiscordUserStatus Status { get; internal set; }

    /// <summary>
    /// Gets the user's old presence.
    /// </summary>
    public DiscordPresence PresenceBefore { get; internal set; }

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
