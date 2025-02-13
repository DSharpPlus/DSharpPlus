namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// Kick
/// </summary>
public sealed class DiscordAuditLogKickEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the kicked member.
    /// </summary>
    public DiscordMember Target { get; internal set; } = default!;

    internal DiscordAuditLogKickEntry() { }
}
