using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <remarks>
/// The combined sum of characters in all <see cref="Title"/>, <see cref="Description"/>, <see cref="InternalEmbedField.Name"/>, <see cref="InternalEmbedField.Value"/>, <see cref="InternalEmbedFooter.Text"/>, and <see cref="InternalEmbedAuthor.Name"/> fields across all embeds attached to a message must not exceed 6000 characters. Violating any of these constraints will result in a Bad Request response.
/// </remarks>
public sealed record InternalEmbed
{
    /// <summary>
    /// The title of embed.
    /// </summary>
    /// <remarks>
    /// Max 256 characters.
    /// </remarks>
    [JsonPropertyName("title")]
    public Optional<string> Title { get; init; }

    /// <summary>
    /// The type of embed (always "rich" for webhook embeds).
    /// </summary>
    [JsonPropertyName("type")]
#pragma warning disable CS0618 // Type or member is obsolete
    public Optional<DiscordEmbedType> Type { get; init; }
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// The description of embed.
    /// </summary>
    /// <remarks>
    /// Max 4096 characters.
    /// </remarks>
    [JsonPropertyName("description")]
    public Optional<string> Description { get; init; }

    /// <summary>
    /// The url of embed.
    /// </summary>
    [JsonPropertyName("url")]
    public Optional<string> Url { get; init; }

    /// <summary>
    /// The timestamp of the embed content.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public Optional<DateTimeOffset> Timestamp { get; init; }

    /// <summary>
    /// The color code of the embed.
    /// </summary>
    [JsonPropertyName("color")]
    public Optional<int> Color { get; init; }

    /// <summary>
    /// Footer information.
    /// </summary>
    [JsonPropertyName("footer")]
    public Optional<InternalEmbedFooter> Footer { get; init; }

    /// <summary>
    /// Image information.
    /// </summary>
    [JsonPropertyName("image")]
    public Optional<InternalEmbedImage> Image { get; init; }

    /// <summary>
    /// Thumbnail information.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public Optional<InternalEmbedThumbnail> Thumbnail { get; init; }

    /// <summary>
    /// Video information.
    /// </summary>
    [JsonPropertyName("video")]
    public Optional<InternalEmbedVideo> Video { get; init; }

    /// <summary>
    /// Provider information.
    /// </summary>
    [JsonPropertyName("provider")]
    public Optional<InternalEmbedProvider> Provider { get; init; }

    /// <summary>
    /// Author information.
    /// </summary>
    [JsonPropertyName("author")]
    public Optional<InternalEmbedAuthor> Author { get; init; }

    /// <summary>
    /// Fields information.
    /// </summary>
    /// <remarks>
    /// Max 25 fields.
    /// </remarks>
    [JsonPropertyName("fields")]
    public Optional<IReadOnlyList<InternalEmbedField>> Fields { get; init; }
}
