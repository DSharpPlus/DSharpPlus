using System;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <summary>
/// Represents a code that when used, creates a guild based on a snapshot of an existing guild.
/// </summary>
public sealed record InternalGuildTemplate
{
    /// <summary>
    /// The template code (unique ID).
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    /// <summary>
    /// The template name.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// The description for the template.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The number of times this template has been used.
    /// </summary>
    [JsonPropertyName("usage_count")]
    public required int UsageCount { get; init; }

    /// <summary>
    /// The ID of the user who created the template.
    /// </summary>
    [JsonPropertyName("creator_id")]
    public required Snowflake CreatorId { get; init; }

    /// <summary>
    /// The user who created the template.
    /// </summary>
    [JsonPropertyName("creator")]
    public required InternalUser Creator { get; init; } 

    /// <summary>
    /// When this template was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// When this template was last synced to the source guild.
    /// </summary>
    [JsonPropertyName("updated_at")]
    public required DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// The ID of the guild this template is based on.
    /// </summary>
    [JsonPropertyName("source_guild_id")]
    public required Snowflake SourceGuildId { get; init; }

    /// <summary>
    /// The guild snapshot this template contains.
    /// </summary>
    [JsonPropertyName("serialized_source_guild")]
    public required InternalGuild SerializedSourceGuild { get; init; }

    /// <summary>
    /// Whether the template has unsynced changes.
    /// </summary>
    [JsonPropertyName("is_dirty")]
    public bool? IsDirty { get; init; }
}
