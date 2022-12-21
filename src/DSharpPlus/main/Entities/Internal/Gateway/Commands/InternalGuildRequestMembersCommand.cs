using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Commands;

/// <summary>
/// Used to request all members for a guild or a list of guilds.
/// </summary>
public sealed record InternalGuildRequestMembersCommand
{
    /// <summary>
    /// The id of the guild to get members for.
    /// </summary>
    /// <remarks>
    /// Always required.
    /// </remarks>
    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; init; } = null!;

    /// <summary>
    /// The string that username starts with, or an empty string to return all members.
    /// </summary>
    /// <remarks>
    /// Required if <see cref="UserIds"/> is not set.
    /// </remarks>
    [JsonPropertyName("query")]
    public Optional<string> Query { get; init; }

    /// <summary>
    /// The maximum number of members to send matching the query; a limit of 0 can be used with an empty string query to return all members.
    /// </summary>
    /// <remarks>
    /// Required when <see cref="Query"/> is set.
    /// </remarks>
    [JsonPropertyName("limit")]
    public int Limit { get; init; }

    /// <summary>
    /// Used to specify if we want the presences of the matched members.
    /// </summary>
    [JsonPropertyName("presences")]
    public Optional<bool> Presences { get; init; }

    /// <summary>
    /// Used to specify which users you wish to fetch.
    /// </summary>
    /// <remarks>
    /// Required if <see cref="Query"/> is not set.
    /// </remarks>
    [JsonPropertyName("user_ids")]
    public Optional<IReadOnlyList<Snowflake>> UserIds { get; init; }

    /// <summary>
    /// Nonce to identify the Guild Members Chunk response.
    /// </summary>
    /// <remarks>
    /// Nonce can only be up to 32 bytes. If you send an invalid nonce it will be ignored and the reply member_chunk(s) will not have a nonce set.
    /// </remarks>
    [JsonPropertyName("nonce")]
    public Optional<string> Nonce { get; init; }
}
