using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordEmbedVideo
    {
        /// <summary>
        /// The source url of video.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Url { get; init; } = null!;

        /// <summary>
        /// A proxied url of the video.
        /// </summary>
        [JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> ProxyUrl { get; init; }

        /// <summary>
        /// The height of video
        /// </summary>
        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> Height { get; init; }

        /// <summary>
        /// The width of video.
        /// </summary>
        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> Width { get; init; }
    }
}
