namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// MemberMove
/// </summary>
public sealed class DiscordAuditLogMemberMoveEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the channel the members were moved in.
    /// </summary>
    public DiscordChannel Channel { get; internal set; } = default!;

    /// <summary>
    /// Gets the amount of users that were moved out from the voice channel.
    /// </summary>
    public int UserCount { get; internal set; }
}
