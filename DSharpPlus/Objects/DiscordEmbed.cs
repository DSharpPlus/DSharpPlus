using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class DiscordEmbed
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
        [JsonProperty("color")]
        public int Color { get; set; }
        [JsonProperty("footer")]
        public DiscordEmbedFooter Footer { get; set; }
        [JsonProperty("image")]
        public DiscordEmbedImage Image { get; set; }
        [JsonProperty("thumbnail")]
        public DiscordEmbedThumbnail Thumbnail { get; set; }
        [JsonProperty("video")]
        public DiscordEmbedVideo Video { get; set; }
        [JsonProperty("provider")]
        public DiscordEmbedProvider Provider { get; set; }
        [JsonProperty("author")]
        public DiscordEmbedAuthor Author { get; set; }
        [JsonProperty("fields")]
        public List<DiscordEmbedField> Fields { get; set; }
    }
}
