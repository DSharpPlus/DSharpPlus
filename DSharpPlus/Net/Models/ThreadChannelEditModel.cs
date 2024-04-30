using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models;

public class ThreadChannelEditModel : ChannelEditModel
{
    /// <summary>
    /// Sets if the thread is archived
    /// </summary>
    public bool? IsArchived { internal get; set; }

    /// <summary>
    /// Sets AutoArchiveDuration of the thread
    /// </summary>
    public DiscordAutoArchiveDuration? AutoArchiveDuration { internal get; set; }

    /// <summary>
    /// Sets if anyone can unarchive a thread
    /// </summary>
    public bool? Locked { internal get; set; }

    /// <summary>
    /// Sets the applied tags for the thread
    /// </summary>
    public IEnumerable<ulong> AppliedTags { internal get; set; }

    /// <summary>
    /// Sets the flags for the channel (Either PINNED or REQUIRE_TAG)
    /// </summary>
    public DiscordChannelFlags? Flags { internal get; set; }

    /// <summary>
    /// Sets whether non-moderators can add other non-moderators to a thread. Only available on private threads
    /// </summary>
    public bool? IsInvitable { internal get; set; }

    internal ThreadChannelEditModel() { }
}
