namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// OverwriteCreate, OverwriteDelete, OverwriteUpdate
/// </summary>
public sealed class DiscordAuditLogOverwriteEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected overwrite. Null if the overwrite was deleted or not in cache.
    /// </summary>
    public DiscordOverwrite? Target { get; internal set; } = default!;

    /// <summary>
    /// Gets the channel for which the overwrite was changed.
    /// </summary>
    public DiscordChannel Channel { get; internal set; } = default!;

    /// <summary>
    /// Gets the description of overwrite's allow value change.
    /// </summary>
    public PropertyChange<DiscordPermissions?> AllowedPermissions { get; internal set; }

    /// <summary>
    /// Gets the description of overwrite's deny value change.
    /// </summary>
    public PropertyChange<DiscordPermissions?> DeniedPermissions { get; internal set; }

    /// <summary>
    /// Gets the description of overwrite's type change.
    /// </summary>
    public PropertyChange<DiscordOverwriteType> Type { get; internal set; }

    /// <summary>
    /// Gets the description of overwrite's target id change.
    /// </summary>
    public PropertyChange<ulong?> TargetIdChange { get; internal set; }

    internal DiscordAuditLogOverwriteEntry() { }
}
