namespace DSharpPlus.Entities.AuditLogs;

using Caching;

public sealed class DiscordAuditLogOverwriteEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected overwrite.
    /// </summary>
    public CachedEntity<ulong, DiscordOverwrite> Target { get; internal set; }

    /// <summary>
    /// Gets the channel for which the overwrite was changed.
    /// </summary>
    public CachedEntity<ulong, DiscordChannel> Channel { get; internal set; }

    /// <summary>
    /// Gets the description of overwrite's allow value change.
    /// </summary>
    public PropertyChange<Permissions?> AllowedPermissions { get; internal set; }

    /// <summary>
    /// Gets the description of overwrite's deny value change.
    /// </summary>
    public PropertyChange<Permissions?> DeniedPermissions { get; internal set; }

    /// <summary>
    /// Gets the description of overwrite's type change.
    /// </summary>
    public PropertyChange<OverwriteType> Type { get; internal set; }

    /// <summary>
    /// Gets the description of overwrite's target id change.
    /// </summary>
    public PropertyChange<ulong?> TargetIdChange { get; internal set; }

    internal DiscordAuditLogOverwriteEntry() { }
}
