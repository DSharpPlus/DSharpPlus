using DSharpPlus.Entities;
namespace DSharpPlus.EventArgs;

/// <summary>
/// Fired when an event is updated.
/// </summary>
public class ScheduledGuildEventUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The event before the update, or null if it wasn't cached.
    /// </summary>
    public DiscordScheduledGuildEvent EventBefore { get; internal set; }

    /// <summary>
    /// The event after the update.
    /// </summary>
    public DiscordScheduledGuildEvent EventAfter { get; internal set; }

    internal ScheduledGuildEventUpdateEventArgs() : base() { }
}
