using DSharpPlus.Caching;

namespace DSharpPlus.Entities.AuditLogs;

public sealed class DiscordAuditLogAutoModerationExecutedEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Name of the rule that was executed
    /// </summary>
    public string ResponsibleRule { get; internal set; } = default!;

    /// <summary>
    /// User that was affected by the rule
    /// </summary>
    public CachedEntity<ulong, DiscordUser> TargetUser { get; internal set; }

    /// <summary>
    /// Type of the trigger that was executed
    /// </summary>
    public DiscordRuleTriggerType RuleTriggerType { get; internal set; }

    /// <summary>
    /// Channel where the rule was executed
    /// </summary>
    public CachedEntity<ulong, DiscordChannel> Channel { get; internal set; }
}
