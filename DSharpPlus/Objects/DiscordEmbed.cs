using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordEmbed
    {
        public DiscordEmbed()
        {
            Fields = new List<DiscordEmbedField>();
        }
        /// <summary>
        /// Title of the embed
        /// </summary>
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }
        /// <summary>
        /// Type of the embed ("rich" for webhook embeds)
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; internal set; }
        /// <summary>
        /// Description of the embed
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
        /// <summary>
        /// Url of the embed
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
        /// <summary>
        /// Timestamp of the embed content
        /// </summary>
        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Timestamp { get; set; }
        /// <summary>
        /// Color code of the embed
        /// </summary>
        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        internal int _color { get; set; }

        public DiscordColor Color {
            get { return new DiscordColor(_color); }
            set { _color = value._color; }
        }
        /// <summary>
        /// Footer information
        /// </summary>
        [JsonProperty("footer", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbedFooter Footer { get; set; }
        /// <summary>
        /// Image information
        /// </summary>
        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbedImage Image { get; set; }
        /// <summary>
        /// Thumbnail information
        /// </summary>
        [JsonProperty("thumbnail", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbedThumbnail Thumbnail { get; set; }
        /// <summary>
        /// Video information
        /// </summary>
        [JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbedVideo Video { get; internal set; }
        /// <summary>
        /// Provider information
        /// </summary>
        [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbedProvider Provider { get; internal set; }
        /// <summary>
        /// Author information
        /// </summary>
        [JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbedAuthor Author { get; set; }
        /// <summary>
        /// Fields information
        /// </summary>
        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public List<DiscordEmbedField> Fields { get; set; }
    }
}
