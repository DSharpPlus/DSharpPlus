using System;
using DSharpPlus.Caching;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ChannelPinsUpdated"/> event.
/// </summary>
public class ChannelPinsUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild in which the update occurred. This field is null if the channel is not a guild channel.
    /// </summary>
    public CachedEntity<ulong, DiscordGuild>? Guild { get; internal set; }
    
    /// <summary>
    /// Gets the channel in which the update occurred.
    /// </summary>
    public CachedEntity<ulong, DiscordChannel> Channel { get; internal set; }

    /// <summary>
    /// Gets the timestamp of the latest pin.
    /// </summary>
    public DateTimeOffset? LastPinTimestamp { get; internal set; }

    internal ChannelPinsUpdateEventArgs() : base() { }
}
