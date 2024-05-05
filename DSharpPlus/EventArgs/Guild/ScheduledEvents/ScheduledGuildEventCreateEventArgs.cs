using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Fired when a new scheduled guild event is created.
/// </summary>
public class ScheduledGuildEventCreateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The guild this event is scheduled for.
    /// </summary>
    public DiscordGuild Guild => this.Event.Guild;

    /// <summary>
    /// The channel this event is scheduled for, if applicable.
    /// </summary>
    public DiscordChannel Channel => this.Event.Channel;

    /// <summary>
    /// The user that created the event.
    /// </summary>
    public DiscordUser Creator => this.Event.Creator;

    /// <summary>
    /// The event that was created.
    /// </summary>
    public DiscordScheduledGuildEvent Event { get; internal set; }

    internal ScheduledGuildEventCreateEventArgs() : base() { }
}
