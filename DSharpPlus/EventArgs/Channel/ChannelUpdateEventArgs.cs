using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ChannelUpdated"/> event.
/// </summary>
public class ChannelUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the post-update channel.
    /// </summary>
    public DiscordChannel ChannelAfter { get; internal set; }

    /// <summary>
    /// Gets the pre-update channel.
    /// </summary>
    /// <remarks>This value comes from cache and is null if its not present in cache</remarks>
    public DiscordChannel? ChannelBefore { get; internal set; }

    /// <summary>
    /// Gets the guild in which the update occurred.
    /// </summary>
    /// <remarks>This value comes from cache and could be null</remarks>
    public DiscordGuild? Guild { get; internal set; }
    
    /// <summary>
    /// Gets the ID of the guild in which the update occurred.
    /// </summary>
    public ulong GuildId { get; internal set; }

    internal ChannelUpdateEventArgs() : base() { }
}
