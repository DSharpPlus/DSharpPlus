using System;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <summary>
/// Represents a sticker that can be sent in messages.
/// </summary>
public sealed record InternalSticker
{
    /// <summary>
    /// The id of the sticker.
    /// </summary>
    [JsonPropertyName("id")]
    public required Snowflake Id { get; init; }

    /// <summary>
    /// For standard stickers, id of the pack the sticker is from.
    /// </summary>
    [JsonPropertyName("pack_id")]
    public Optional<Snowflake> PackId { get; init; }

    /// <summary>
    /// The name of the sticker.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// The description of the sticker.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Autocomplete/suggestion tags for the sticker (max 200 characters).
    /// </summary>
    /// <remarks>
    /// A comma separated list of keywords is the format used in this field by standard stickers, but this is just a convention. Incidentally the client will always use a name generated from an emoji as the value of this field when creating or modifying a guild sticker.
    /// </remarks>
    [JsonPropertyName("tags")]
    public required string Tags { get; init; }

    /// <summary>
    /// Deprecated previously the sticker asset hash, now an empty string
    /// </summary>
    [JsonPropertyName("asset")]
    [Obsolete("Deprecated previously the sticker asset hash, now an empty string")]
    public Optional<string> Asset { get; set; }

    /// <summary>
    /// The type of sticker.
    /// </summary>
    [JsonPropertyName("type")]
    public required DiscordStickerType Type { get; init; }

    /// <summary>
    /// The type of sticker format.
    /// </summary>
    [JsonPropertyName("format_type")]
    public required DiscordStickerFormatType FormatType { get; init; }

    /// <summary>
    /// Whether this guild sticker can be used, may be false due to loss of Server Boosts.
    /// </summary>
    [JsonPropertyName("available")]
    public Optional<bool> Available { get; init; }

    /// <summary>
    /// The id of the guild that owns this sticker.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Optional<Snowflake?> GuildId { get; init; }

    /// <summary>
    /// The user that uploaded the guild sticker.
    /// </summary>
    [JsonPropertyName("user")]
    public Optional<InternalUser> User { get; init; }

    /// <summary>
    /// The standard sticker's sort order within its pack.
    /// </summary>
    [JsonPropertyName("sort_value")]
    public Optional<int> SortValue { get; init; }
}
