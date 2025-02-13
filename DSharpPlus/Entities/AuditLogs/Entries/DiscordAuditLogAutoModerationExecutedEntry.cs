namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// AutoModerationBlockMessage, AutoModerationFlagToChannel, AutoModerationUserCommunicationDisabled
/// </summary>
public sealed class DiscordAuditLogAutoModerationExecutedEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Name of the rule that was executed
    /// </summary>
    public string ResponsibleRule { get; internal set; } = default!;

    /// <summary>
    /// User that was affected by the rule
    /// </summary>
    public DiscordUser TargetUser { get; internal set; } = default!;

    /// <summary>
    /// Type of the trigger that was executed
    /// </summary>
    public DiscordRuleTriggerType RuleTriggerType { get; internal set; }

    /// <summary>
    /// Channel where the rule was executed
    /// </summary>
    public DiscordChannel Channel { get; internal set; } = default!;
}
