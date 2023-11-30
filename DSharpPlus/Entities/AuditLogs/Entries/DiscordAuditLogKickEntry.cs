using DSharpPlus.Caching;

namespace DSharpPlus.Entities.AuditLogs;

public sealed class DiscordAuditLogKickEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the kicked member.
    /// </summary>
    public CachedEntity<ulong, DiscordMember> Target { get; internal set; }

    internal DiscordAuditLogKickEntry() { }
}
