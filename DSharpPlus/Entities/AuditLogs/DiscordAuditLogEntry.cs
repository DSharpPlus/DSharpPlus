namespace DSharpPlus.Entities.AuditLogs;

using Caching;

/// <summary>
/// Represents an audit log entry. All fields of type <see cref="PropertyChange{T}"/> will be null if the property was not changed.
/// </summary>
public abstract class DiscordAuditLogEntry : SnowflakeObject
{
    /// <summary>
    /// Gets the entry's guild.
    /// </summary>
    public CachedEntity<ulong, DiscordGuild> Guild { get; internal set; }
    
    /// <summary>
    /// Gets the entry's action type.
    /// </summary>
    public DiscordAuditLogActionType ActionType { get; internal set; }

    /// <summary>
    /// Gets the user responsible for the action.
    /// </summary>
    public CachedEntity<ulong, DiscordUser> UserResponsible { get; internal set; }

    /// <summary>
    /// Gets the reason defined in the action.
    /// </summary>
    public string? Reason { get; internal set; }

    /// <summary>
    /// Gets the category under which the action falls.
    /// </summary>
    public DiscordAuditLogActionCategory ActionCategory { get; internal set; }
}
