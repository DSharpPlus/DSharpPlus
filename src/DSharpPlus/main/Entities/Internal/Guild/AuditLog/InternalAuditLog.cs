using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record InternalAuditLog
    {
        /// <summary>
        /// A list of <see cref="InternalAuditLogEntry"/>, sorted from most to least recent.
        /// </summary>
        [JsonPropertyName("audit_log_entries")]
        public IReadOnlyList<InternalAuditLogEntry> AuditLogEntries { get; init; } = Array.Empty<InternalAuditLogEntry>();

        /// <summary>
        /// List of auto moderation rules referenced in the audit log.
        /// </summary>
        [JsonPropertyName("auto_moderation_rules")]
        public IReadOnlyList<InternalAutoModerationRule> AutoModerationRules { get; init; } = Array.Empty<InternalAutoModerationRule>();

        /// <summary>
        /// A list of <see cref="InternalGuildScheduledEvent"/> referenced in the audit log.
        /// </summary>
        [JsonPropertyName("guild_scheduled_events")]
        public IReadOnlyList<InternalGuildScheduledEvent> GuildScheduledEvents { get; init; } = Array.Empty<InternalGuildScheduledEvent>();

        /// <summary>
        /// A list of partial <see cref="InternalIntegration"/> objects.
        /// </summary>
        [JsonPropertyName("integrations")]
        public IReadOnlyList<InternalIntegration> Integrations { get; init; } = Array.Empty<InternalIntegration>();

        /// <summary>
        /// A list of threads referenced in the audit log.
        /// </summary>
        /// <remarks>
        /// * Threads referenced in THREAD_CREATE and THREAD_UPDATE events are included in the threads map, since archived threads might not be kept in memory by clients.
        /// </remarks>
        [JsonPropertyName("threads")]
        public IReadOnlyList<InternalChannel> Threads { get; init; } = Array.Empty<InternalChannel>();

        /// <summary>
        /// A list of <see cref="InternalUser"/> referenced in the audit log.
        /// </summary>
        [JsonPropertyName("users")]
        public IReadOnlyList<InternalUser> Users { get; init; } = Array.Empty<InternalUser>();

        /// <summary>
        /// A list of webhooks referenced in the audit log.
        /// </summary>
        [JsonPropertyName("webhooks")]
        public IReadOnlyList<InternalWebhook> Webhooks { get; init; } = Array.Empty<InternalWebhook>();
    }
}
