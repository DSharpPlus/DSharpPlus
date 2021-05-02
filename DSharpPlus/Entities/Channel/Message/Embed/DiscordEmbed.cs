using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a discord embed.
    /// </summary>
    public sealed class DiscordEmbed
    {
        /// <summary>
        /// Gets the embed's title.
        /// </summary>
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; internal set; }

        /// <summary>
        /// Gets the embed's type.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; internal set; }

        /// <summary>
        /// Gets the embed's description.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the embed's url.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; internal set; }

        /// <summary>
        /// Gets the embed's timestamp.
        /// </summary>
        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Timestamp { get; internal set; }

        /// <summary>
        /// Gets the embed's color.
        /// </summary>
        [JsonIgnore]
        public Optional<DiscordColor> Color
            => this._colorLazy.Value;

        [JsonProperty("color", NullValueHandling = NullValueHandling.Include)]
        internal Optional<int> _color;
        [JsonIgnore]
        private readonly Lazy<Optional<DiscordColor>> _colorLazy;

        /// <summary>
        /// Gets the embed's footer.
        /// </summary>
        [JsonProperty("footer", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbedFooter Footer { get; internal set; }

        /// <summary>
        /// Gets the embed's image.
        /// </summary>
        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbedImage Image { get; internal set; }

        /// <summary>
        /// Gets the embed's thumbnail.
        /// </summary>
        [JsonProperty("thumbnail", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbedThumbnail Thumbnail { get; internal set; }

        /// <summary>
        /// Gets the embed's video.
        /// </summary>
        [JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbedVideo Video { get; internal set; }

        /// <summary>
        /// Gets the embed's provider.
        /// </summary>
        [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbedProvider Provider { get; internal set; }

        /// <summary>
        /// Gets the embed's author.
        /// </summary>
        [JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbedAuthor Author { get; internal set; }

        /// <summary>
        /// Gets the embed's fields.
        /// </summary>
        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordEmbedField> Fields { get; internal set; }

        internal DiscordEmbed()
        {
            this._colorLazy = new Lazy<Optional<DiscordColor>>(() => this._color.HasValue ? Optional.FromValue<DiscordColor>(this._color.Value) : Optional.FromNoValue<DiscordColor>());
        }
    }
}
