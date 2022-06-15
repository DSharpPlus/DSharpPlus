using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordAuditLogEntryInfo
    {
        /// <summary>
        /// The ID of the app whose permissions were targeted.
        /// </summary>
        [JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake ApplicationId { get; init; } = null!;

        /// <summary>
        /// The channel in which the entities were targeted.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The number of entities that were targeted.
        /// </summary>
        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
        public string Count { get; init; } = null!;

        /// <summary>
        /// The number of days after which inactive members were kicked.
        /// </summary>
        [JsonProperty("delete_member_days", NullValueHandling = NullValueHandling.Ignore)]
        public string DeleteMemberDays { get; init; } = null!;

        /// <summary>
        /// The id of the overwritten entity.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The number of members removed by the prune.
        /// </summary>
        [JsonProperty("members_removed", NullValueHandling = NullValueHandling.Ignore)]
        public string MembersRemoved { get; init; } = null!;

        /// <summary>
        /// The id of the message that was targeted.
        /// </summary>
        [JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake MessageId { get; init; } = null!;

        /// <summary>
        /// The name of the role if <see cref="Type"/> is "0" (not present if <see cref="Type"/> is "1").
        /// </summary>
        [JsonProperty("role_name", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> RoleName { get; init; } = null!;

        /// <summary>
        /// The type of overwritten entity - "0" for "role" or "1" for "member".
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; init; } = null!;

        public static implicit operator ulong(DiscordAuditLogEntryInfo auditLogEntryInfo) => auditLogEntryInfo.Id;
        public static implicit operator DiscordSnowflake(DiscordAuditLogEntryInfo auditLogEntryInfo) => auditLogEntryInfo.Id;
    }
}
