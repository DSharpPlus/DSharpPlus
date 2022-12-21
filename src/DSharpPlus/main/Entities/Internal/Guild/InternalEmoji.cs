using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <summary>
/// Implements a <see href="https://discord.com/developers/docs/resources/emoji#emoji-object">Internal emoji</see>.
/// </summary>
public sealed record InternalEmoji
{
    /// <summary>
    /// The emoji's Id.
    /// </summary>
    [JsonPropertyName("id")]
    public InternalSnowflake? Id { get; init; } = null!;

    /// <summary>
    /// The emoji's name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; } = null!;

    /// <summary>
    /// The roles allowed to use this emoji.
    /// </summary>
    [JsonPropertyName("roles")]
    public Optional<IReadOnlyList<InternalSnowflake>> Roles { get; init; }

    /// <summary>
    /// The user that created this emoji.
    /// </summary>
    [JsonPropertyName("user")]
    public Optional<InternalUser> User { get; init; } = null!;

    /// <summary>
    /// Whether this emoji must be wrapped in colons.
    /// </summary>
    [JsonPropertyName("require_colons")]
    public Optional<bool> RequiresColons { get; init; }

    /// <summary>
    /// Whether this emoji is managed.
    /// </summary>
    [JsonPropertyName("managed")]
    public Optional<bool> Managed { get; init; }

    /// <summary>
    /// Whether this emoji is animated.
    /// </summary>
    [JsonPropertyName("animated")]
    public Optional<bool> Animated { get; init; }

    /// <summary>
    /// Whether this emoji can be used. May be false due to loss of Server Boosts.
    /// </summary>
    [JsonPropertyName("available")]
    public Optional<bool> Available { get; init; }

    /// <exception cref="NullReferenceException">If the emoji does not have an id.</exception>
    public static implicit operator ulong(InternalEmoji emoji) => emoji.Id!;

    /// <exception cref="NullReferenceException">If the emoji does not have an id.</exception>
    public static implicit operator InternalSnowflake(InternalEmoji emoji) => emoji.Id!;
}
