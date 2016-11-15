using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordGuildEmbed
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
        [JsonProperty("channel_id")]
        public ulong ChannelID { get; set; }
    }
}
