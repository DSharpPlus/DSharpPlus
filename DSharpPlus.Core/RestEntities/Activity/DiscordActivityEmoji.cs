using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordActivityEmoji
    {
        /// <summary>
        /// The name of the emoji.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The id of the emoji.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> Id { get; init; }

        /// <summary>
        /// Whether this emoji is animated.
        /// </summary>
        [JsonProperty("animated", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Animated { get; init; }
    }
}
