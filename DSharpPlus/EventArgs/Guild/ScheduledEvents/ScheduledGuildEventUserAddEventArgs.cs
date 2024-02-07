using DSharpPlus.Entities;
namespace DSharpPlus.EventArgs;

/// <summary>
/// Fired when someone subscribes to the scheduled event.
/// </summary>
public class ScheduledGuildEventUserAddEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The guild the event is scheduled for.
    /// </summary>
    public DiscordGuild? Guild { get; internal set; }
    
    /// <summary>
    /// The id of the guild the event is scheduled for.
    /// </summary>
    public ulong GuildId { get; internal set; }

    /// <summary>
    /// The event that was subscribed to.
    /// </summary>
    public DiscordScheduledGuildEvent? Event { get; internal set; }
    
    /// <summary>
    /// The id of the event that was subscribed to.
    /// </summary>
    public ulong EventId { get; internal set; }

    /// <summary>
    /// The user that subscribed to the event.
    /// </summary>
    public DiscordUser User { get; internal set; }

    internal ScheduledGuildEventUserAddEventArgs() : base() { }
}
