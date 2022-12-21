using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record InternalEmbedThumbnail
    {
        /// <summary>
        /// The source url of thumbnail (only supports http(s) and attachments).
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; init; } = null!;

        /// <summary>
        /// A proxied url of the thumbnail.
        /// </summary>
        [JsonPropertyName("proxy_url")]
        public Optional<string> ProxyUrl { get; init; }

        /// <summary>
        /// The height of thumbnail.
        /// </summary>
        [JsonPropertyName("height")]
        public Optional<int> Height { get; init; }

        /// <summary>
        /// The width of thumbnail.
        /// </summary>
        [JsonPropertyName("width")]
        public Optional<int> Width { get; init; }
    }
}
