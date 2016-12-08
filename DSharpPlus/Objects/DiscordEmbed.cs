using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordEmbed
    {
        /// <summary>
        /// Title of the embed
        /// </summary>
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }
        /// <summary>
        /// Type of the embed ("rich" for webhook embeds)
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
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
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Color code of the embed
        /// </summary>
        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public int Color { get; set; }
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
        public DiscordEmbedVideo Video { get; set; }
        /// <summary>
        /// Provider information
        /// </summary>
        [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbedProvider Provider { get; set; }
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
