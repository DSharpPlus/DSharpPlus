using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordGuildEmbed
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; internal set; }
        [JsonProperty("channel_id")]
        public ulong ChannelID { get; internal set; }
    }
}
