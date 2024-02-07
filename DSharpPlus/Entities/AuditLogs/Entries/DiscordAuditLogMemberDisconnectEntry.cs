namespace DSharpPlus.Entities.AuditLogs;

public sealed class DiscordAuditLogMemberDisconnectEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the amount of users that were disconnected from the voice channel.
    /// </summary>
    public int UserCount { get; internal set; }
}
