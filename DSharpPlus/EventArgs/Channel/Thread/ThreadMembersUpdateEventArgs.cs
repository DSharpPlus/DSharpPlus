namespace DSharpPlus.EventArgs;
using System.Collections.Generic;
using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ThreadMembersUpdated"/> event.
/// </summary>
public class ThreadMembersUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the approximate number of members in the thread, capped at 50.
    /// </summary>
    public int MemberCount { get; internal set; }

    /// <summary>
    /// Gets the members who were removed from the thread. These could be skeleton objects depending on cache state.
    /// </summary>
    public IReadOnlyList<DiscordMember> RemovedMembers { get; internal set; }

    /// <summary>
    /// Gets the members who were added to the thread.
    /// </summary>
    public IReadOnlyList<DiscordThreadChannelMember> AddedMembers { get; internal set; }

    /// <summary>
    /// Gets the thread associated with the member changes.
    /// </summary>
    public DiscordThreadChannel Thread { get; internal set; }

    /// <summary>
    /// Gets the guild.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal ThreadMembersUpdateEventArgs() : base() { }
}
