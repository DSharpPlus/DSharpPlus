using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordReaction
    {
        /// <summary>
        /// Times this emoji has been used to react.
        /// </summary>
        [JsonPropertyName("count")]
        public int Count { get; init; }

        /// <summary>
        /// Whether the current user reacted using this emoji.
        /// </summary>
        [JsonPropertyName("me")]
        public bool Me { get; init; }

        /// <summary>
        /// The emoji information.
        /// </summary>
        [JsonPropertyName("emoji")]
        public DiscordEmoji Emoji { get; init; } = null!;
    }
}
