using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ThreadListSynced"/> event.
/// </summary>
public class ThreadListSyncEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets all thread member objects, indicating which threads the current user has been added to.
    /// </summary>
    public IReadOnlyList<DiscordThreadChannelMember> CurrentMembers { get; internal set; }

    /// <summary>
    /// Gets all active threads in the given channels that the current user can access.
    /// </summary>
    public IReadOnlyList<DiscordThreadChannel> Threads { get; internal set; }

    /// <summary>
    /// Gets the parent channels whose threads are being synced. May contain channels that have no active threads as well.
    /// </summary>
    public IReadOnlyList<DiscordChannel> Channels { get; internal set; }

    /// <summary>
    /// Gets the guild being synced.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal ThreadListSyncEventArgs() : base() { }
}
