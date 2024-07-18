using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for the ChannelDeleted event.
/// </summary>
public class ChannelDeletedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the channel that was deleted.
    /// </summary>
    public DiscordChannel Channel { get; internal set; }

    /// <summary>
    /// Gets the guild this channel belonged to.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal ChannelDeletedEventArgs() : base() { }
}
