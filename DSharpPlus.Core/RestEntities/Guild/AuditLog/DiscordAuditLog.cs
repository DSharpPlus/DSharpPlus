using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordAuditLog
    {
        /// <summary>
        /// A list of <see cref="DiscordAuditLogEntry"/>, sorted from most to least recent.
        /// </summary>
        [JsonPropertyName("audit_log_entries")]
        public IReadOnlyList<DiscordAuditLogEntry> AuditLogEntries { get; init; } = Array.Empty<DiscordAuditLogEntry>();

        /// <summary>
        /// List of auto moderation rules referenced in the audit log.
        /// </summary>
        [JsonPropertyName("auto_moderation_rules")]
        public IReadOnlyList<DiscordAutoModerationRule> AutoModerationRules { get; init; } = Array.Empty<DiscordAutoModerationRule>();

        /// <summary>
        /// A list of <see cref="DiscordGuildScheduledEvent"/> referenced in the audit log.
        /// </summary>
        [JsonPropertyName("guild_scheduled_events")]
        public IReadOnlyList<DiscordGuildScheduledEvent> GuildScheduledEvents { get; init; } = Array.Empty<DiscordGuildScheduledEvent>();

        /// <summary>
        /// A list of partial <see cref="DiscordIntegration"/> objects.
        /// </summary>
        [JsonPropertyName("integrations")]
        public IReadOnlyList<DiscordIntegration> Integrations { get; init; } = Array.Empty<DiscordIntegration>();

        /// <summary>
        /// A list of threads referenced in the audit log.
        /// </summary>
        /// <remarks>
        /// * Threads referenced in THREAD_CREATE and THREAD_UPDATE events are included in the threads map, since archived threads might not be kept in memory by clients.
        /// </remarks>
        [JsonPropertyName("threads")]
        public IReadOnlyList<DiscordChannel> Threads { get; init; } = Array.Empty<DiscordChannel>();

        /// <summary>
        /// A list of <see cref="DiscordUser"/> referenced in the audit log.
        /// </summary>
        [JsonPropertyName("users")]
        public IReadOnlyList<DiscordUser> Users { get; init; } = Array.Empty<DiscordUser>();

        /// <summary>
        /// A list of webhooks referenced in the audit log.
        /// </summary>
        [JsonPropertyName("webhooks")]
        public IReadOnlyList<DiscordWebhook> Webhooks { get; init; } = Array.Empty<DiscordWebhook>();
    }
}
