using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalAuditLog
{
    /// <summary>
    /// A list of <see cref="InternalAuditLogEntry"/>, sorted from most to least recent.
    /// </summary>
    [JsonPropertyName("audit_log_entries")]
    public required IReadOnlyList<InternalAuditLogEntry> AuditLogEntries { get; init; } 

    /// <summary>
    /// List of auto moderation rules referenced in the audit log.
    /// </summary>
    [JsonPropertyName("auto_moderation_rules")]
    public required IReadOnlyList<InternalAutoModerationRule> AutoModerationRules { get; init; } 

    /// <summary>
    /// A list of <see cref="InternalGuildScheduledEvent"/> referenced in the audit log.
    /// </summary>
    [JsonPropertyName("guild_scheduled_events")]
    public required IReadOnlyList<InternalGuildScheduledEvent> GuildScheduledEvents { get; init; } 

    /// <summary>
    /// A list of partial <see cref="InternalIntegration"/> objects.
    /// </summary>
    [JsonPropertyName("integrations")]
    public required IReadOnlyList<InternalIntegration> Integrations { get; init; } 

    /// <summary>
    /// A list of threads referenced in the audit log.
    /// </summary>
    /// <remarks>
    /// * Threads referenced in THREAD_CREATE and THREAD_UPDATE events are included in the threads map, since archived 
    /// threads might not be kept in memory by clients.
    /// </remarks>
    [JsonPropertyName("threads")]
    public required IReadOnlyList<InternalChannel> Threads { get; init; } 

    /// <summary>
    /// A list of <see cref="InternalUser"/> referenced in the audit log.
    /// </summary>
    [JsonPropertyName("users")]
    public required IReadOnlyList<InternalUser> Users { get; init; } 

    /// <summary>
    /// A list of webhooks referenced in the audit log.
    /// </summary>
    [JsonPropertyName("webhooks")]
    public required IReadOnlyList<InternalWebhook> Webhooks { get; init; } 
}
