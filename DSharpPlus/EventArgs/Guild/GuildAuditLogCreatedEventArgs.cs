using DSharpPlus.Entities;
using DSharpPlus.Entities.AuditLogs;

namespace DSharpPlus.EventArgs;

using Caching;

public class GuildAuditLogCreatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Created audit log entry.
    /// </summary>
    public DiscordAuditLogEntry AuditLogEntry { get; internal set; }

    /// <summary>
    /// Guild where audit log entry was created.
    /// </summary>
    public CachedEntity<ulong, DiscordGuild> Guild { get; internal set; }

    internal GuildAuditLogCreatedEventArgs() : base() { }
}
