using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when anyone is added to or removed from a thread. If the current user does not have the <see cref="Enums.DiscordGatewayIntents.GuildMembers"/>, then this event will only be sent if the current user was added to or removed from the thread.
/// </summary>
/// <remarks>
/// In this gateway event, the thread member objects will also include the <see cref="InternalGuildMember"/> and nullable <see cref="InternalUpdatePresencePayload"/> for each added thread member.
/// </remarks>
public sealed record InternalThreadMembersUpdatePayload
{
    /// <summary>
    /// The id of the thread.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; init; } = null!;

    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; init; } = null!;

    /// <summary>
    /// The approximate number of members in the thread, capped at 50.
    /// </summary>
    [JsonPropertyName("member_count")]
    public int MemberCount { get; init; }

    /// <summary>
    /// The users who were added to the thread.
    /// </summary>
    [JsonPropertyName("added_members")]
    public Optional<IReadOnlyList<InternalThreadMember>> AddedMembers { get; init; }

    /// <summary>
    /// The id of the users who were removed from the thread.
    /// </summary>
    [JsonPropertyName("removed_member_ids")]
    public Optional<IReadOnlyList<Snowflake>> RemovedMemberIds { get; init; }
}
