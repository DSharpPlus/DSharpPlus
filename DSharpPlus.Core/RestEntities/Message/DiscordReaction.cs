using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordReaction
    {
        /// <summary>
        /// Times this emoji has been used to react.
        /// </summary>
        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
        public int Count { get; init; }

        /// <summary>
        /// Whether the current user reacted using this emoji.
        /// </summary>
        [JsonProperty("me", NullValueHandling = NullValueHandling.Ignore)]
        public bool Me { get; init; }

        /// <summary>
        /// The emoji information.
        /// </summary>
        [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmoji Emoji { get; init; } = null!;
    }
}
