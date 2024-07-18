using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for the ChannelUpdated event.
/// </summary>
public class ChannelUpdatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the post-update channel.
    /// </summary>
    public DiscordChannel ChannelAfter { get; internal set; }

    /// <summary>
    /// Gets the pre-update channel.
    /// </summary>
    public DiscordChannel ChannelBefore { get; internal set; }

    /// <summary>
    /// Gets the guild in which the update occurred.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal ChannelUpdatedEventArgs() : base() { }
}
