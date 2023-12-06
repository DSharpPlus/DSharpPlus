using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

using Caching;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.MessageDeleted"/> event.
/// </summary>
public class MessageDeleteEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the message that was deleted.
    /// </summary>
    public CachedEntity<ulong, DiscordMessage> Message { get; internal set; }

    /// <summary>
    /// Gets the channel this message belonged to.
    /// </summary>
    public CachedEntity<ulong, DiscordChannel> Channel { get; internal set; }

    /// <summary>
    /// Gets the guild this message belonged to. This property is null for DM channels.
    /// </summary>
    public CachedEntity<ulong, DiscordGuild>? Guild { get; internal set; }

    internal MessageDeleteEventArgs() : base() { }
}
