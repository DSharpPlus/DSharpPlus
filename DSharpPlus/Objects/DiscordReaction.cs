using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordReaction
    {
        /// <summary>
        /// Times this emoji has been used to react
        /// </summary>
        [JsonProperty("count")]
        public int Count { get; internal set; }
        /// <summary>
        /// Whether the current user reacted using this emoji
        /// </summary>
        [JsonProperty("me")]
        public bool Me { get; internal set; }
        /// <summary>
        /// Emoji information
        /// </summary>
        [JsonProperty("emoji")]
        public DiscordEmoji Emoji { get; internal set; }
    }
}
