using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordAuditLogEntry
    {
        /// <summary>
        /// The id of the affected entity (webhook, user, role, etc.)
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string? TargetId { get; init; }

        /// <summary>
        /// The changes made to the <see cref="TargetId"/>
        /// </summary>
        [JsonProperty("changes", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyList<DiscordAuditLogChange>> Changes { get; init; }

        /// <summary>
        /// The user who made the changes.
        /// </summary>
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake? UserId { get; init; }

        /// <summary>
        /// The id of the entry.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The type of action that occurred.
        /// </summary>
        [JsonProperty("action_type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordAuditLogEvent ActionType { get; init; }

        /// <summary>
        /// Additional info for certain action types.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordAuditLogEntryInfo> Options { get; init; }

        /// <summary>
        /// The reason for the change (0-512 characters)
        /// </summary>
        [JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Reason { get; init; }

        public static implicit operator ulong(DiscordAuditLogEntry auditLogEntry) => auditLogEntry.Id;
        public static implicit operator DiscordSnowflake(DiscordAuditLogEntry auditLogEntry) => auditLogEntry.Id;
    }
}
