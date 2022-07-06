using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordActivityEmoji
    {
        /// <summary>
        /// The name of the emoji.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The id of the emoji.
        /// </summary>
        [JsonPropertyName("id")]
        public Optional<DiscordSnowflake> Id { get; init; }

        /// <summary>
        /// Whether this emoji is animated.
        /// </summary>
        [JsonPropertyName("animated")]
        public Optional<bool> Animated { get; init; }
    }
}
