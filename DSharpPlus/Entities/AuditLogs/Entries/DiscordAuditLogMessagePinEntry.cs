using DSharpPlus.Caching;

namespace DSharpPlus.Entities.AuditLogs;

public sealed class DiscordAuditLogMessagePinEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected message's user.
    /// </summary>
    public CachedEntity<ulong,DiscordUser> Target { get; internal set; }

    /// <summary>
    /// Gets the channel the message is in.
    /// </summary>
    public CachedEntity<ulong,DiscordChannel> Channel { get; internal set; }

    /// <summary>
    /// Gets the message the pin action was for.
    /// </summary>
    public CachedEntity<ulong, DiscordMessage> Message { get; internal set; }

    internal DiscordAuditLogMessagePinEntry() { }
}
