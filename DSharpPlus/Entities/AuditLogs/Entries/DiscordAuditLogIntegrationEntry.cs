namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// IntegrationCreate, IntegrationDelete, IntegrationUpdate
/// </summary>
public sealed class DiscordAuditLogIntegrationEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the description of emoticons' change.
    /// </summary>
    public PropertyChange<bool?> EnableEmoticons { get; internal set; }

    /// <summary>
    /// Gets the description of expire grace period's change.
    /// </summary>
    public PropertyChange<int?> ExpireGracePeriod { get; internal set; }

    /// <summary>
    /// Gets the description of expire behavior change.
    /// </summary>
    public PropertyChange<int?> ExpireBehavior { get; internal set; }

    /// <summary>
    /// Gets the type of the integration.
    /// </summary>
    public PropertyChange<string> Type { get; internal set; }

    /// <summary>
    /// Gets the name of the integration.
    /// </summary>
    public PropertyChange<string> Name { get; internal set; }
}
