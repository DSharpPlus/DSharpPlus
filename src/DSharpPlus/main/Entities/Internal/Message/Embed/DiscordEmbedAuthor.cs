using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordEmbedAuthor
    {
        /// <summary>
        /// The name of the author.
        /// </summary>
        /// <remarks>
        /// Max 256 characters.
        /// </remarks>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The url of the author.
        /// </summary>
        [JsonPropertyName("url")]
        public Optional<string> Url { get; init; }

        /// <summary>
        /// The url of the author's icon (only supports http(s) and attachments).
        /// </summary>
        [JsonPropertyName("icon_url")]
        public Optional<string> IconUrl { get; init; }

        /// <summary>
        /// A proxied url of the author's icon.
        /// </summary>
        [JsonPropertyName("proxy_icon_url")]
        public Optional<string> ProxyIconUrl { get; init; }
    }
}
