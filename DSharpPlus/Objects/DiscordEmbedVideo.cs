using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordEmbedVideo
    {
        [JsonProperty("url")]
        public string Url { get; internal set; }
        [JsonProperty("height")]
        public int Height { get; internal set; }
        [JsonProperty("width")]
        public int Width { get; internal set; }
    }
}
