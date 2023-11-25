using System.Collections.Generic;
using DSharpPlus.Enums;

namespace DSharpPlus.Entities.AuditLogs;

using Caching;

public sealed class DiscordAuditLogAutoModerationRuleEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Id of the rule
    /// </summary>
    public PropertyChange<ulong?> RuleId { get; internal set; }

    /// <summary>
    /// Id of the guild where the rule was changed
    /// </summary>
    public PropertyChange<ulong?> GuildId { get; internal set; }

    /// <summary>
    /// Name of the rule
    /// </summary>
    public PropertyChange<string?> Name { get; internal set; }

    /// <summary>
    /// Id of the user that created the rule
    /// </summary>
    public PropertyChange<ulong?> CreatorId { get; internal set; }

    /// <summary>
    /// Indicates in what event context a rule should be checked.
    /// </summary>
    public PropertyChange<RuleEventType?> EventType { get; internal set; }

    /// <summary>
    /// Characterizes the type of content which can trigger the rule.
    /// </summary>
    public PropertyChange<RuleTriggerType?> TriggerType { get; internal set; }

    /// <summary>
    /// Additional data used to determine whether a rule should be triggered. 
    /// </summary>
    public PropertyChange<DiscordRuleTriggerMetadata?> TriggerMetadata { get; internal set; }

    /// <summary>
    /// Actions which will execute when the rule is triggered.
    /// </summary>
    public PropertyChange<IEnumerable<DiscordAutoModerationAction>?> Actions { get; internal set; }

    /// <summary>
    /// Whether the rule is enabled or not.
    /// </summary>
    public PropertyChange<bool?> Enabled { get; internal set; }

    /// <summary>
    /// Roles that should not be affected by the rule
    /// </summary>
    public PropertyChange<IEnumerable<CachedEntity<ulong,DiscordRole>>?> ExemptRoles { get; internal set; }

    /// <summary>
    /// Channels that should not be affected by the rule
    /// </summary>
    public PropertyChange<IEnumerable<CachedEntity<ulong,DiscordChannel>>?> ExemptChannels { get; internal set; }

    /// <summary>
    /// List of trigger keywords that were added to the rule
    /// </summary>
    public IEnumerable<string>? AddedKeywords { get; internal set; }

    /// <summary>
    /// List of trigger keywords that were removed from the rule
    /// </summary>
    public IEnumerable<string>? RemovedKeywords { get; internal set; }

    /// <summary>
    /// List of trigger regex patterns that were added to the rule
    /// </summary>
    public IEnumerable<string>? AddedRegexPatterns { get; internal set; }

    /// <summary>
    /// List of trigger regex patterns that were removed from the rule
    /// </summary>
    public IEnumerable<string>? RemovedRegexPatterns { get; internal set; }

    /// <summary>
    /// List of strings that were added to the allow list
    /// </summary>
    public IEnumerable<string>? AddedAllowList { get; internal set; }

    /// <summary>
    /// List of strings that were removed from the allow list
    /// </summary>
    public IEnumerable<string>? RemovedAllowList { get; internal set; }
}
