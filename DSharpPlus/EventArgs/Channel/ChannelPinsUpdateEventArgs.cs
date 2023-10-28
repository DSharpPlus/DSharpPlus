using System;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ChannelPinsUpdated"/> event.
/// </summary>
public class ChannelPinsUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild in which the update occurred.
    /// </summary>
    /// <remarks>This value comes from cache and is null if its not present in cache</remarks>
    public DiscordGuild? Guild { get; internal set; }

    /// <summary>
    /// Gets the ID of the guild in which the update occurred.
    /// </summary>
    public ulong GuildId { get; internal set; }
    
    /// <summary>
    /// Gets the channel in which the update occurred.
    /// </summary>
    /// <remarks>This value comes from cache and is null if its not present in cache</remarks>
    public DiscordChannel? Channel { get; internal set; }
    
    /// <summary>
    /// Gets the ID of the channel in which the update occurred.
    /// </summary>
    public ulong ChannelId { get; internal set; }

    /// <summary>
    /// Gets the timestamp of the latest pin.
    /// </summary>
    public DateTimeOffset? LastPinTimestamp { get; internal set; }

    internal ChannelPinsUpdateEventArgs() : base() { }
}
