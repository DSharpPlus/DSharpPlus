namespace DSharpPlus.Entities.AuditLogs;

public sealed class DiscordAuditLogRoleUpdateEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected role.
    /// </summary>
    public DiscordRole Target { get; internal set; } = default!;

    /// <summary>
    /// Gets the description of role's name change.
    /// </summary>
    public PropertyChange<string> NameChange { get; internal set; }

    /// <summary>
    /// Gets the description of role's color change.
    /// </summary>
    public PropertyChange<int?> ColorChange { get; internal set; }

    /// <summary>
    /// Gets the description of role's permission set change.
    /// </summary>
    public PropertyChange<DiscordPermissions?> PermissionChange { get; internal set; }

    /// <summary>
    /// Gets the description of the role's position change.
    /// </summary>
    public PropertyChange<int?> PositionChange { get; internal set; }

    /// <summary>
    /// Gets the description of the role's mentionability change.
    /// </summary>
    public PropertyChange<bool?> MentionableChange { get; internal set; }

    /// <summary>
    /// Gets the description of the role's hoist status change.
    /// </summary>
    public PropertyChange<bool?> HoistChange { get; internal set; }

    internal DiscordAuditLogRoleUpdateEntry() { }
}
