using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordEmbedProvider
    {
        /// <summary>
        /// The name of provider.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Name { get; init; }

        /// <summary>
        /// The url of provider.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Url { get; init; }
    }
}
