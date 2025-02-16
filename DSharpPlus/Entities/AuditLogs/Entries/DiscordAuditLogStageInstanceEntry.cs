namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// StageInstanceCreate, StageInstanceUpdate, StageInstanceDelete
/// </summary>
public sealed class DiscordAuditLogStageInstanceEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected stage instance 
    /// </summary>
    public DiscordStageInstance Target { get; set; }

    public PropertyChange<string> Topic { get; set; }


    public PropertyChange<DiscordStagePrivacyLevel> PrivacyLevel { get; set; }

    internal DiscordAuditLogStageInstanceEntry() { }
}
