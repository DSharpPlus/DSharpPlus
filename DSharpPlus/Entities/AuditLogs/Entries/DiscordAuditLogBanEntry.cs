namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// Ban, Unban
/// </summary>
public sealed class DiscordAuditLogBanEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the banned member.
    /// </summary>
    public DiscordMember Target { get; internal set; } = default!;

    internal DiscordAuditLogBanEntry() { }
}
