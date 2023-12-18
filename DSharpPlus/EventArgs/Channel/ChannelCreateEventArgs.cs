using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ChannelCreated"/> event.
/// </summary>
public class ChannelCreateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the channel that was created.
    /// </summary>
    public DiscordChannel Channel { get; internal set; }

    /// <summary>
    /// Gets the guild in which the channel was created.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal ChannelCreateEventArgs() : base() { }
}
