using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a reaction to a message.
    /// </summary>
    public class DiscordReaction : PropertyChangedBase
    {
        private int _count;
        private bool _isMe;

        /// <summary>
        /// Gets the total number of users who reacted with this emoji.
        /// </summary>
        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
        public int Count { get => _count; internal set => OnPropertySet(ref _count, value); }

        /// <summary>
        /// Gets whether the current user reacted with this emoji.
        /// </summary>
        [JsonProperty("me", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsMe { get => _isMe; internal set => OnPropertySet(ref _isMe, value); }

        /// <summary>
        /// Gets the emoji used to react to this message.
        /// </summary>
        [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmoji Emoji { get; internal set; }

        internal DiscordReaction() { }
    }
}
