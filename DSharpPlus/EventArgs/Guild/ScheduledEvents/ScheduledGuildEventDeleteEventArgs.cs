using DSharpPlus.Entities;
namespace DSharpPlus.EventArgs;

/// <summary>
/// Fired when an event is deleted.
/// </summary>
public class ScheduledGuildEventDeleteEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The event that was deleted.
    /// </summary>
    public DiscordScheduledGuildEvent Event { get; internal set; }

    internal ScheduledGuildEventDeleteEventArgs() : base() { }
}
