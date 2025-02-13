namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// Prune
/// </summary>
public sealed class DiscordAuditLogPruneEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the number inactivity days after which members were pruned.
    /// </summary>
    public int Days { get; internal set; }

    /// <summary>
    /// Gets the number of members pruned.
    /// </summary>
    public int Toll { get; internal set; }

    internal DiscordAuditLogPruneEntry() { }
}

