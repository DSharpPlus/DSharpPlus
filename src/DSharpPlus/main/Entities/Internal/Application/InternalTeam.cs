using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalTeam
{
    /// <summary>
    /// A hash of the image of the team's icon.
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    /// <summary>
    /// The unique id of the team.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; init; } = null!;

    /// <summary>
    /// The members of the team.
    /// </summary>
    [JsonPropertyName("members")]
    public IReadOnlyList<InternalTeamMember> Members { get; init; } = Array.Empty<InternalTeamMember>();

    /// <summary>
    /// The name of the team.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// The user id of the current team owner.
    /// </summary>
    [JsonPropertyName("owner_user_id")]
    public Snowflake OwnerUserId { get; init; } = null!;

    public static implicit operator ulong(InternalTeam team) => team.Id;
    public static implicit operator Snowflake(InternalTeam team) => team.Id;
}
