namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// MemberDisconnect
/// </summary>
public sealed class DiscordAuditLogMemberDisconnectEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the amount of users that were disconnected from the voice channel.
    /// </summary>
    public int UserCount { get; internal set; }
}
