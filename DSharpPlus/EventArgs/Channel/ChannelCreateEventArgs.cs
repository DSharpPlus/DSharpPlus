using DSharpPlus.Entities;
using DSharpPlus.Caching;

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
    /// Gets the guild in which the channel was created. This field is null if the channel has no guild.
    /// </summary>
    public CachedEntity<ulong,DiscordGuild>? Guild { get; internal set; }

    internal ChannelCreateEventArgs() : base() { }
}
