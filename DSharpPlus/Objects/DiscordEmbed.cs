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
        [JsonProperty("title")]
        public string Title { get; set; }
        /// <summary>
        /// Type of the embed ("rich" for webhook embeds)
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
        /// <summary>
        /// Description of the embed
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }
        /// <summary>
        /// Url of the embed
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
        /// <summary>
        /// Timestamp of the embed content
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Color code of the embed
        /// </summary>
        [JsonProperty("color")]
        public int Color { get; set; }
        /// <summary>
        /// Footer information
        /// </summary>
        [JsonProperty("footer")]
        public DiscordEmbedFooter Footer { get; set; }
        /// <summary>
        /// Image information
        /// </summary>
        [JsonProperty("image")]
        public DiscordEmbedImage Image { get; set; }
        /// <summary>
        /// Thumbnail information
        /// </summary>
        [JsonProperty("thumbnail")]
        public DiscordEmbedThumbnail Thumbnail { get; set; }
        /// <summary>
        /// Video information
        /// </summary>
        [JsonProperty("video")]
        public DiscordEmbedVideo Video { get; set; }
        /// <summary>
        /// Provider information
        /// </summary>
        [JsonProperty("provider")]
        public DiscordEmbedProvider Provider { get; set; }
        /// <summary>
        /// Author information
        /// </summary>
        [JsonProperty("author")]
        public DiscordEmbedAuthor Author { get; set; }
        /// <summary>
        /// Fields information
        /// </summary>
        [JsonProperty("fields")]
        public List<DiscordEmbedField> Fields { get; set; }
    }
}
