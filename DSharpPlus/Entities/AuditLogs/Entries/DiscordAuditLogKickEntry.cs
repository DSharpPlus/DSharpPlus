namespace DSharpPlus.Entities.AuditLogs;

using Caching;

public sealed class DiscordAuditLogKickEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the kicked member.
    /// </summary>
    public CachedEntity<ulong, DiscordMember> Target { get; internal set; }

    internal DiscordAuditLogKickEntry() { }
}
