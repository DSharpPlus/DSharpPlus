using System;
using System.Collections.Generic;

namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// MemberUpdate, MemberRoleUpdate
/// </summary>
public sealed class DiscordAuditLogMemberUpdateEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected member.
    /// </summary>
    public DiscordMember Target { get; internal set; } = default!;

    /// <summary>
    /// Gets the description of member's nickname change.
    /// </summary>
    public PropertyChange<string> NicknameChange { get; internal set; }

    /// <summary>
    /// Gets the roles that were removed from the member.
    /// </summary>
    public IReadOnlyList<DiscordRole>? RemovedRoles { get; internal set; }

    /// <summary>
    /// Gets the roles that were added to the member.
    /// </summary>
    public IReadOnlyList<DiscordRole>? AddedRoles { get; internal set; }

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
