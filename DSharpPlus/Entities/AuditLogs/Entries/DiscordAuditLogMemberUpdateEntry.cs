using System;
using System.Collections.Generic;

namespace DSharpPlus.Entities.AuditLogs;

using Caching;

public sealed class DiscordAuditLogMemberUpdateEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected member.
    /// </summary>
    public CachedEntity<ulong, DiscordMember> Target { get; internal set; }

    /// <summary>
    /// Gets the description of member's nickname change.
    /// </summary>
    public PropertyChange<string> NicknameChange { get; internal set; }

    /// <summary>
    /// Gets the roles that were removed from the member.
    /// </summary>
    public IReadOnlyList<CachedEntity<ulong, DiscordRole>> RemovedRoles { get; internal set; }

    /// <summary>
    /// Gets the roles that were added to the member.
    /// </summary>
    public IReadOnlyList<CachedEntity<ulong, DiscordRole>> AddedRoles { get; internal set; }

    /// <summary>
    /// Gets the description of member's mute status change.
    /// </summary>
    public PropertyChange<bool?> MuteChange { get; internal set; }

    /// <summary>
    /// Gets the description of member's deaf status change.
    /// </summary>
    public PropertyChange<bool?> DeafenChange { get; internal set; }

    /// <summary>
    /// Gets the change in a user's timeout status
    /// </summary>
    public PropertyChange<DateTime?> TimeoutChange { get; internal set; }

    internal DiscordAuditLogMemberUpdateEntry() { }
}
