using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Objects
{
    public struct DiscordEmbed
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
        [JsonProperty("provider")]
        public DiscordEmbedProvider Provider { get; set; }
        [JsonProperty("author")]
        public DiscordEmbedAuthor Author { get; set; }
        [JsonProperty("fields")]
        public List<DiscordEmbedField> Fields { get; set; }
    }

    public struct DiscordEmbedThumbnail
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("proxy_url")]
        public string ProxyUrl { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
    }

    public struct DiscordEmbedImage
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("proxy_url")]
        public string ProxyUrl { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
    }

    public struct DiscordEmbedProvider
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public struct DiscordEmbedAuthor
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }
        [JsonProperty("proxy_icon_url")]
        public string ProxyIconUrl { get; set; }
    }

    public struct DiscordEmbedFooter
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }
        [JsonProperty("proxy_icon_url")]
        public string ProxyIconUrl { get; set; }
    }

    public struct DiscordEmbedField
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
        [JsonProperty("inline")]
        public bool Inline { get; set; }
    }
}
