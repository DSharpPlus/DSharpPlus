using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for the ChannelCreated event.
/// </summary>
public class ChannelCreatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the channel that was created.
    /// </summary>
    public DiscordChannel Channel { get; internal set; }

    /// <summary>
    /// Gets the guild in which the channel was created.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal ChannelCreatedEventArgs() : base() { }
}
