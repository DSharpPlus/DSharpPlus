using DSharpPlus.Caching;

namespace DSharpPlus.Entities.AuditLogs;

public sealed class DiscordAuditLogMemberMoveEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the channel the members were moved in.
    /// </summary>
    public CachedEntity<ulong, DiscordChannel> Channel { get; internal set; }

    /// <summary>
    /// Gets the amount of users that were moved out from the voice channel.
    /// </summary>
    public int UserCount { get; internal set; }
}
