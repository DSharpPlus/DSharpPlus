using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordEmbedImage
    {
        /// <summary>
        /// The source url of the image (only supports http(s) and attachments).
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; init; } = null!;

        /// <summary>
        /// A proxied url of the image.
        /// </summary>
        [JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> ProxyUrl { get; init; }

        /// <summary>
        /// The height of image.
        /// </summary>
        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> Height { get; init; }

        /// <summary>
        /// The width of image.
        /// </summary>
        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> Width { get; init; }
    }
}
