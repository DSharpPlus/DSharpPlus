using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordGuildEmbed
    {
        /// <summary>
        /// If the embed is enabled
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
        /// <summary>
        /// The embed channel id
        /// </summary>
        [JsonProperty("channel_id")]
        public ulong ChannelID { get; set; }
    }
}
