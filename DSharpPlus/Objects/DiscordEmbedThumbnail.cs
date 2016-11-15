using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordEmbedThumbnail
    {
        [JsonProperty("url")]
        public string Url { get; internal set; }
        [JsonProperty("proxy_url")]
        public string ProxyUrl { get; internal set; }
        [JsonProperty("height")]
        public int Height { get; internal set; }
        [JsonProperty("width")]
        public int Width { get; internal set; }
    }
}
