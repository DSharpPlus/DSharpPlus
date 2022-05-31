using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordEmbedAuthor
    {
        /// <summary>
        /// The name of the author.
        /// </summary>
        /// <remarks>
        /// Max 256 characters.
        /// </remarks>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The url of the author.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Url { get; init; }

        /// <summary>
        /// The url of the author's icon (only supports http(s) and attachments).
        /// </summary>
        [JsonProperty("icon_url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> IconUrl { get; init; }

        /// <summary>
        /// A proxied url of the author's icon.
        /// </summary>
        [JsonProperty("proxy_icon_url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> ProxyIconUrl { get; init; }
    }
}
