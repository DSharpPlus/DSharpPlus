using Newtonsoft.Json;

namespace DSharpPlus.Objects
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordReaction
    {
        /// <summary>
        /// Times this emoji has been used to react
        /// </summary>
        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
        public int Count { get; internal set; }
        /// <summary>
        /// Whether the current user reacted using this emoji
        /// </summary>
        [JsonProperty("me", NullValueHandling = NullValueHandling.Ignore)]
        public bool Me { get; internal set; }
        /// <summary>
        /// Emoji information
        /// </summary>
        [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmoji Emoji { get; internal set; }
    }
}
