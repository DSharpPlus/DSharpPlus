using System.Collections.Generic;

namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// MessageDelete, MessageBulkDelete
/// </summary>
public sealed class DiscordAuditLogMessageEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected User.
    /// </summary>
    public DiscordUser Target { get; internal set; } = default!;

    /// <summary>
    /// Gets the affected Member. This is null if the action was performed on a user that is not in the member cache.
    /// </summary>
    public DiscordMember? Member => this.Channel.Guild.Members.GetValueOrDefault(this.Target.Id);

    /// <summary>
    /// Gets the channel in which the action occurred.
    /// </summary>
    public DiscordChannel Channel { get; internal set; } = default!;

    /// <summary>
    /// Gets the number of messages that were affected.
    /// </summary>
    public int? MessageCount { get; internal set; }

    internal DiscordAuditLogMessageEntry() { }
}
