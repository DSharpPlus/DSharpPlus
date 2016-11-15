using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordEmbedProvider
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("url")]
        public string Url { get; internal set; }
    }
}
