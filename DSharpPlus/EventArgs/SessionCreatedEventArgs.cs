using System.Collections.Generic;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for SessionCreated event.
/// </summary>
public sealed class SessionCreatedEventArgs : DiscordEventArgs
{
    internal SessionCreatedEventArgs() : base() { }

    /// <summary>
    /// The ID of the shard this event occurred on.
    /// </summary>
    public int ShardId { get; internal set; }

    /// <summary>
    /// Gets the IDs of guilds connected to the shard that created this session. Note that DiscordGuild objects may
    /// not yet be available by the time this event is fired, and if you require access to any objects, you should wait
    /// for GuildDownloadCompleted.
    /// </summary>
    public IReadOnlyList<ulong> GuildIds { get; internal set; }
}
