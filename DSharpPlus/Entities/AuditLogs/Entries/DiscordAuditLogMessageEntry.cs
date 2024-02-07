using DSharpPlus.Caching;

namespace DSharpPlus.Entities.AuditLogs;

public sealed class DiscordAuditLogMessageEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected User. This can be null if this entry was created by bulk deleting messages.
    /// </summary>
    public CachedEntity<ulong, DiscordUser>? Target { get; internal set; }

    /// <summary>
    /// Gets the affected Member.
    /// </summary>
    public CachedEntity<ulong, DiscordMember> Member;

    /// <summary>
    /// Gets the channel in which the action occurred.
    /// </summary>
    public CachedEntity<ulong, DiscordChannel> Channel { get; internal set; }

    /// <summary>
    /// Gets the number of messages that were affected.
    /// </summary>
    public int? MessageCount { get; internal set; }

    internal DiscordAuditLogMessageEntry() { }
}
