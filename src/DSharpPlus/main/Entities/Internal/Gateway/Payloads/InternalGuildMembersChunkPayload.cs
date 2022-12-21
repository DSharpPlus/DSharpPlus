using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent in response to Guild Request Members. You can use the chunk_index and chunk_count to calculate how many chunks are left for your request.
/// </summary>
public sealed record InternalGuildMembersChunkPayload
{
    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; init; } = null!;

    /// <summary>
    /// A set of guild members.
    /// </summary>
    [JsonPropertyName("members")]
    public IReadOnlyList<InternalGuildMember> Members { get; init; } = Array.Empty<InternalGuildMember>();

    /// <summary>
    /// The chunk index in the expected chunks for this response (0 <= chunk_index < chunk_count).
    /// </summary>
    [JsonPropertyName("chunk_index")]
    public int ChunkIndex { get; init; }

    /// <summary>
    /// The total number of expected chunks for this response.
    /// </summary>
    [JsonPropertyName("chunk_count")]
    public int ChunkCount { get; init; }

    /// <summary>
    /// If passing an invalid id to REQUEST_GUILD_MEMBERS, it will be returned here.
    /// </summary>
    [JsonPropertyName("not_found")]
    public Optional<IReadOnlyList<Snowflake>> NotFound { get; init; }

    /// <summary>
    /// If passing true to REQUEST_GUILD_MEMBERS, presences of the returned members will be here.
    /// </summary>
    [JsonPropertyName("presences")]
    public Optional<InternalUpdatePresencePayload> Presences { get; init; }

    /// <summary>
    /// The nonce used in the Guild Members Request.
    /// </summary>
    [JsonPropertyName("nonce")]
    public Optional<string> Nonce { get; init; }
}
