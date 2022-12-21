using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record InternalAuditLogEntry
    {
        /// <summary>
        /// The id of the affected entity (webhook, user, role, etc.)
        /// </summary>
        /// <remarks>
        /// For <see cref="InternalAuditLogEvent.ApplicationCommandPermissionUpdate"/> events, the <see cref="TargetId"/> is the command ID or the app ID since the changes array represents the entire permissions property on the guild permissions object.
        /// </remarks>
        [JsonPropertyName("id")]
        public string? TargetId { get; init; }

        /// <summary>
        /// The changes made to the <see cref="TargetId"/>
        /// </summary>
        [JsonPropertyName("changes")]
        public Optional<IReadOnlyList<InternalAuditLogChange>> Changes { get; init; }

        /// <summary>
        /// The user who made the changes.
        /// </summary>
        [JsonPropertyName("user_id")]
        public InternalSnowflake? UserId { get; init; }

        /// <summary>
        /// The id of the entry.
        /// </summary>
        [JsonPropertyName("id")]
        public InternalSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The type of action that occurred.
        /// </summary>
        [JsonPropertyName("action_type")]
        public InternalAuditLogEvent ActionType { get; init; }

        /// <summary>
        /// Additional info for certain action types.
        /// </summary>
        [JsonPropertyName("options")]
        public Optional<InternalAuditLogEntryInfo> Options { get; init; }

        /// <summary>
        /// The reason for the change (0-512 characters)
        /// </summary>
        [JsonPropertyName("reason")]
        public Optional<string> Reason { get; init; }

        public static implicit operator ulong(InternalAuditLogEntry auditLogEntry) => auditLogEntry.Id;
        public static implicit operator InternalSnowflake(InternalAuditLogEntry auditLogEntry) => auditLogEntry.Id;
    }
}
