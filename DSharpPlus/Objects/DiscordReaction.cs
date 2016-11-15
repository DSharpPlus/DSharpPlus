using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordReaction
    {
        [JsonProperty("count")]
        public int Count { get; internal set; }
        [JsonProperty("me")]
        public bool Me { get; internal set; }
        [JsonProperty("emoji")]
        public DiscordEmoji Emoji { get; internal set; }
    }
}
