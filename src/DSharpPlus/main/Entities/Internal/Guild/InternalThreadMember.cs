using System;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <summary>
/// A thread member is used to indicate whether a user has joined a thread or not.
/// </summary>
/// <remarks>
/// The <see cref="Id"/> and <see cref="UserId"/> fields are omitted on the member sent within each thread in the <c>GUILD_CREATE</c> event
/// </remarks>
public sealed record InternalThreadMember
{
    /// <summary>
    /// The id of the thread.
    /// </summary>
    [JsonPropertyName("id")]
    public Optional<Snowflake> Id { get; init; }

    /// <summary>
    /// The id of the user.
    /// </summary>
    [JsonPropertyName("user_id")]
    public Optional<Snowflake> UserId { get; init; }

    /// <summary>
    /// The time the current user last joined the thread.
    /// </summary>
    [JsonPropertyName("join_timestamp")]
    public required DateTimeOffset JoinTimestamp { get; init; }

    /// <summary>
    /// Any user-thread settings, currently only used for notifications.
    /// </summary>
    [JsonPropertyName("flags")]
    public required int Flags { get; init; }

    /// <summary>
    /// The id of the guild.
    /// </summary>
    /// <remarks>
    /// Only sent on the ThreadMemberUpdate gateway payload.
    /// </remarks>
    [JsonPropertyName("guild_id")]
    public Optional<Snowflake> GuildId { get; init; }
}
