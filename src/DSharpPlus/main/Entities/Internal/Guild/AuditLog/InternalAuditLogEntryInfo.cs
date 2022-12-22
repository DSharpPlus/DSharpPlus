using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalAuditLogEntryInfo
{
    /// <summary>
    /// The ID of the app whose permissions were targeted.
    /// </summary>
    [JsonPropertyName("application_id")]
    public Optional<Snowflake> ApplicationId { get; init; } 

    /// <summary>
    /// The channel in which the entities were targeted.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public Optional<Snowflake> ChannelId { get; init; }

    /// <summary>
    /// The number of entities that were targeted.
    /// </summary>
    [JsonPropertyName("count")]
    public Optional<string> Count { get; init; }

    /// <summary>
    /// The number of days after which inactive members were kicked.
    /// </summary>
    [JsonPropertyName("delete_member_days")]
    public Optional<string> DeleteMemberDays { get; init; }

    /// <summary>
    /// The id of the overwritten entity.
    /// </summary>
    [JsonPropertyName("id")]
    public Optional<Snowflake> Id { get; init; } 

    /// <summary>
    /// The number of members removed by the prune.
    /// </summary>
    [JsonPropertyName("members_removed")]
    public Optional<string> MembersRemoved { get; init; }

    /// <summary>
    /// The id of the message that was targeted.
    /// </summary>
    [JsonPropertyName("message_id")]
    public Optional<Snowflake> MessageId { get; init; } 

    /// <summary>
    /// The name of the role if <see cref="Type"/> is "0" (not present if <see cref="Type"/> is "1").
    /// </summary>
    [JsonPropertyName("role_name")]
    public Optional<string> RoleName { get; init; }

    /// <summary>
    /// The type of overwritten entity - "0" for "role" or "1" for "member".
    /// </summary>
    [JsonPropertyName("type")]
    public Optional<string> Type { get; init; }
}
