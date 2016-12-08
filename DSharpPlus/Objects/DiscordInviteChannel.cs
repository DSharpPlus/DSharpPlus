using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordInviteChannel : SnowflakeObject
    {
        /// <summary>
        /// Name of the channel
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("name")]
>>>>>>> master
        public string Name { get; internal set; }
        /// <summary>
        /// "Text" or "Voice"
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("type")]
>>>>>>> master
        public ChannelType Type { get; internal set; }
    }
}
