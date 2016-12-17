using Newtonsoft.Json;

namespace DSharpPlus.Objects
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordGuildEmbed
    {
        /// <summary>
        /// If the embed is enabled
        /// </summary>
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool Enabled { get; set; }
        /// <summary>
        /// The embed channel id
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ChannelID { get; set; }
    }
}
