using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordEmbedAuthor
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("url")]
        public string Url { get; internal set; }
        [JsonProperty("icon_url")]
        public string IconUrl { get; internal set; }
        [JsonProperty("proxy_icon_url")]
        public string ProxyIconUrl { get; internal set; }
    }
}
