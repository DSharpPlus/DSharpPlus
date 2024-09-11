using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

/// <summary>
/// Specifies the parameters for modifying a thread channel.
/// </summary>
/// <remarks>
/// If an <see cref="Optional{T}"/> parameter is not specified, it's state will be left unchanged.
/// </remarks>
public class ThreadChannelEditModel : ChannelEditModel
{
    /// <summary>
    /// Sets if the thread is archived
    /// </summary>
    public bool? IsArchived { get; set; }

    /// <summary>
    /// Sets AutoArchiveDuration of the thread
    /// </summary>
    public DiscordAutoArchiveDuration? AutoArchiveDuration { get; set; }

    /// <summary>
    /// Sets if anyone can unarchive a thread
    /// </summary>
    public bool? Locked { get; set; }

    /// <summary>
    /// Sets the applied tags for the thread
    /// </summary>
    public List<ulong> AppliedTags { get; set; }

    /// <summary>
    /// Sets the flags for the channel (Either PINNED or REQUIRE_TAG)
    /// </summary>
    public new DiscordChannelFlags? Flags { get; set; }

    /// <summary>
    /// Sets whether non-moderators can add other non-moderators to a thread. Only available on private threads
    /// </summary>
    public bool? IsInvitable { get; set; }

    internal ThreadChannelEditModel() { }
}
