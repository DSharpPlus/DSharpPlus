using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class DiscordEmbed
    {
        [JsonProperty("title")]
        public string Title { get; internal set; }
        [JsonProperty("type")]
        public string Type { get; internal set; }
        [JsonProperty("description")]
        public string Description { get; internal set; }
        [JsonProperty("url")]
        public string Url { get; internal set; }
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; internal set; }
        [JsonProperty("color")]
        public int Color { get; internal set; }
        [JsonProperty("footer")]
        public DiscordEmbedFooter Footer { get; internal set; }
        [JsonProperty("image")]
        public DiscordEmbedImage Image { get; internal set; }
        [JsonProperty("thumbnail")]
        public DiscordEmbedThumbnail Thumbnail { get; internal set; }
        [JsonProperty("video")]
        public DiscordEmbedVideo Video { get; internal set; }
        [JsonProperty("provider")]
        public DiscordEmbedProvider Provider { get; internal set; }
        [JsonProperty("author")]
        public DiscordEmbedAuthor Author { get; internal set; }
        [JsonProperty("fields")]
        public List<DiscordEmbedField> Fields { get; internal set; }
    }
}
