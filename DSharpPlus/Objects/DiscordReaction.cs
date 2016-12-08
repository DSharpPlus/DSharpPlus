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
<<<<<<< HEAD
        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("count")]
>>>>>>> master
        public int Count { get; internal set; }
        /// <summary>
        /// Whether the current user reacted using this emoji
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("me", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("me")]
>>>>>>> master
        public bool Me { get; internal set; }
        /// <summary>
        /// Emoji information
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("emoji")]
>>>>>>> master
        public DiscordEmoji Emoji { get; internal set; }
    }
}
