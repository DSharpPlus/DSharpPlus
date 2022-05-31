using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordEmbedFooter
    {
        /// <summary>
        /// The footer text.
        /// </summary>
        /// <remarks>
        /// Max 2048 characters.
        /// </remarks>
        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; init; } = null!;

        /// <summary>
        /// The url of footer icon (only supports http(s) and attachments).
        /// </summary>
        [JsonProperty("icon_url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> IconUrl { get; init; }

        /// <summary>
        /// A proxied url of footer icon.
        /// </summary>
        [JsonProperty("proxy_icon_url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> ProxyIconUrl { get; init; }
    }
}
