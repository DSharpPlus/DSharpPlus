using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordEmbedThumbnail
    {
        /// <summary>
        /// The source url of thumbnail (only supports http(s) and attachments).
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; init; } = null!;

        /// <summary>
        /// A proxied url of the thumbnail.
        /// </summary>
        [JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> ProxyUrl { get; init; }

        /// <summary>
        /// The height of thumbnail.
        /// </summary>
        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> Height { get; init; }

        /// <summary>
        /// The width of thumbnail.
        /// </summary>
        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> Width { get; init; }
    }
}
