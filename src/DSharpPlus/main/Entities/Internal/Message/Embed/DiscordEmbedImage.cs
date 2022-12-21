using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordEmbedImage
    {
        /// <summary>
        /// The source url of the image (only supports http(s) and attachments).
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; init; } = null!;

        /// <summary>
        /// A proxied url of the image.
        /// </summary>
        [JsonPropertyName("proxy_url")]
        public Optional<string> ProxyUrl { get; init; }

        /// <summary>
        /// The height of image.
        /// </summary>
        [JsonPropertyName("height")]
        public Optional<int> Height { get; init; }

        /// <summary>
        /// The width of image.
        /// </summary>
        [JsonPropertyName("width")]
        public Optional<int> Width { get; init; }
    }
}
