using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Fired when someone unsubcribes from an event.
/// </summary>
public class ScheduledGuildEventUserRemoveEventArgs : ScheduledGuildEventUserAddEventArgs
{
    /// <summary>
    /// The guild the event is scheduled for.
    /// </summary>
    public new DiscordGuild Guild => Event.Guild;

    /// <summary>
    /// The event that was unsubscribed from.
    /// </summary>
    public new DiscordScheduledGuildEvent Event { get; internal set; }

    /// <summary>
    /// The user that unsubscribed from the event.
    /// </summary>
    public new DiscordUser User { get; internal set; }
    internal ScheduledGuildEventUserRemoveEventArgs() : base() { }
}
