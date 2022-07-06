using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordEmbedProvider
    {
        /// <summary>
        /// The name of provider.
        /// </summary>
        [JsonPropertyName("name")]
        public Optional<string> Name { get; init; }

        /// <summary>
        /// The url of provider.
        /// </summary>
        [JsonPropertyName("url")]
        public Optional<string> Url { get; init; }
    }
}
