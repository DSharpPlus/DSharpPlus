
using DSharpPlus.Entities;
using DSharpPlus.Entities.AuditLogs;

namespace DSharpPlus.EventArgs;
public class GuildAuditLogCreatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Created audit log entry.
    /// </summary>
    public DiscordAuditLogEntry AuditLogEntry { get; internal set; }

    /// <summary>
    /// Guild where audit log entry was created.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal GuildAuditLogCreatedEventArgs() : base() { }
}
