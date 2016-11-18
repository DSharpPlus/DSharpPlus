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
        [JsonProperty("name")]
        public string Name { get; internal set; }
        /// <summary>
        /// "Text" or "Voice"
        /// </summary>
        [JsonProperty("type")]
        public ChannelType Type { get; internal set; }
    }
}
