using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalAuditLogEntryInfo
{
    /// <summary>
    /// The ID of the app whose permissions were targeted.
    /// </summary>
    [JsonPropertyName("application_id")]
    public Snowflake ApplicationId { get; init; } = null!;

    /// <summary>
    /// The channel in which the entities were targeted.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public Snowflake ChannelId { get; init; } = null!;

    /// <summary>
    /// The number of entities that were targeted.
    /// </summary>
    [JsonPropertyName("count")]
    public string Count { get; init; } = null!;

    /// <summary>
    /// The number of days after which inactive members were kicked.
    /// </summary>
    [JsonPropertyName("delete_member_days")]
    public string DeleteMemberDays { get; init; } = null!;

    /// <summary>
    /// The id of the overwritten entity.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; init; } = null!;

    /// <summary>
    /// The number of members removed by the prune.
    /// </summary>
    [JsonPropertyName("members_removed")]
    public string MembersRemoved { get; init; } = null!;

    /// <summary>
    /// The id of the message that was targeted.
    /// </summary>
    [JsonPropertyName("message_id")]
    public Snowflake MessageId { get; init; } = null!;

    /// <summary>
    /// The name of the role if <see cref="Type"/> is "0" (not present if <see cref="Type"/> is "1").
    /// </summary>
    [JsonPropertyName("role_name")]
    public Optional<string> RoleName { get; init; } = null!;

    /// <summary>
    /// The type of overwritten entity - "0" for "role" or "1" for "member".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = null!;

    public static implicit operator ulong(InternalAuditLogEntryInfo auditLogEntryInfo) => auditLogEntryInfo.Id;
    public static implicit operator Snowflake(InternalAuditLogEntryInfo auditLogEntryInfo) => auditLogEntryInfo.Id;
}
