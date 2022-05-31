using System;
using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <remarks>
    /// The combined sum of characters in all <see cref="Title"/>, <see cref="Description"/>, <see cref="DiscordEmbedField.Name"/>, <see cref="DiscordEmbedField.Value"/>, <see cref="DiscordEmbedFooter.Text"/>, and <see cref="DiscordEmbedAuthor.Name"/> fields across all embeds attached to a message must not exceed 6000 characters. Violating any of these constraints will result in a Bad Request response.
    /// </remarks>
    public sealed record DiscordEmbed
    {
        /// <summary>
        /// The title of embed.
        /// </summary>
        /// <remarks>
        /// Max 256 characters.
        /// </remarks>
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Title { get; init; }

        /// <summary>
        /// The type of embed (always "rich" for webhook embeds).
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
#pragma warning disable CS0618 // Type or member is obsolete
        public Optional<DiscordEmbedType> Type { get; init; } // I tried to use [SupressMessage] here, however it seemed to be ignored.
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// The description of embed.
        /// </summary>
        /// <remarks>
        /// Max 4096 characters.
        /// </remarks>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Description { get; init; }

        /// <summary>
        /// The url of embed.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Url { get; init; }

        /// <summary>
        /// The timestamp of the embed content.
        /// </summary>
        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DateTimeOffset> Timestamp { get; init; }

        /// <summary>
        /// The color code of the embed.
        /// </summary>
        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> Color { get; init; }

        /// <summary>
        /// Footer information.
        /// </summary>
        [JsonProperty("footer", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordEmbedFooter> Footer { get; init; }

        /// <summary>
        /// Image information.
        /// </summary>
        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordEmbedImage> Image { get; init; }

        /// <summary>
        /// Thumbnail information.
        /// </summary>
        [JsonProperty("thumbnail", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordEmbedThumbnail> Thumbnail { get; init; }

        /// <summary>
        /// Video information.
        /// </summary>
        [JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordEmbedVideo> Video { get; init; }

        /// <summary>
        /// Provider information.
        /// </summary>
        [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordEmbedProvider> Provider { get; init; }

        /// <summary>
        /// Author information.
        /// </summary>
        [JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordEmbedAuthor> Author { get; init; }

        /// <summary>
        /// Fields information.
        /// </summary>
        /// <remarks>
        /// Max 25 fields.
        /// </remarks>
        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyList<DiscordEmbedField>> Fields { get; init; }
    }
}
