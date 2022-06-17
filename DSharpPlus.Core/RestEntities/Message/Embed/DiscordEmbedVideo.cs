using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordEmbedVideo
    {
        /// <summary>
        /// The source url of video.
        /// </summary>
        [JsonPropertyName("url")]
        public Optional<string> Url { get; init; } = null!;

        /// <summary>
        /// A proxied url of the video.
        /// </summary>
        [JsonPropertyName("proxy_url")]
        public Optional<string> ProxyUrl { get; init; }

        /// <summary>
        /// The height of video
        /// </summary>
        [JsonPropertyName("height")]
        public Optional<int> Height { get; init; }

        /// <summary>
        /// The width of video.
        /// </summary>
        [JsonPropertyName("width")]
        public Optional<int> Width { get; init; }
    }
}
