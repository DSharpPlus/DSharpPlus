using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordAuditLog
    {
        /// <summary>
        /// A list of <see cref="DiscordAuditLogEntry"/>, sorted from most to least recent.
        /// </summary>
        [JsonProperty("audit_log_entries", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordAuditLogEntry> AuditLogEntries { get; init; } = Array.Empty<DiscordAuditLogEntry>();

        /// <summary>
        /// List of auto moderation rules referenced in the audit log.
        /// </summary>
        [JsonProperty("auto_moderation_rules", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordAutoModerationRule> AutoModerationRules { get; init; } = Array.Empty<DiscordAutoModerationRule>();

        /// <summary>
        /// A list of <see cref="DiscordGuildScheduledEvent"/> referenced in the audit log.
        /// </summary>
        [JsonProperty("guild_scheduled_events", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordGuildScheduledEvent> GuildScheduledEvents { get; init; } = Array.Empty<DiscordGuildScheduledEvent>();

        /// <summary>
        /// A list of partial <see cref="DiscordIntegration"/> objects.
        /// </summary>
        [JsonProperty("integrations", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordIntegration> Integrations { get; init; } = Array.Empty<DiscordIntegration>();

        /// <summary>
        /// A list of threads referenced in the audit log.
        /// </summary>
        /// <remarks>
        /// * Threads referenced in THREAD_CREATE and THREAD_UPDATE events are included in the threads map, since archived threads might not be kept in memory by clients.
        /// </remarks>
        [JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordChannel> Threads { get; init; } = Array.Empty<DiscordChannel>();

        /// <summary>
        /// A list of <see cref="DiscordUser"/> referenced in the audit log.
        /// </summary>
        [JsonProperty("users", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordUser> Users { get; init; } = Array.Empty<DiscordUser>();

        /// <summary>
        /// A list of webhooks referenced in the audit log.
        /// </summary>
        [JsonProperty("webhooks", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordWebhook> Webhooks { get; init; } = Array.Empty<DiscordWebhook>();
    }
}
