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
<<<<<<< HEAD
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("enabled")]
>>>>>>> master
        public bool Enabled { get; set; }
        /// <summary>
        /// The embed channel id
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("channel_id")]
>>>>>>> master
        public ulong ChannelID { get; set; }
    }
}
