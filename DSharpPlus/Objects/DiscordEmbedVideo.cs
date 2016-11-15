using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordEmbedVideo
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
    }
}
