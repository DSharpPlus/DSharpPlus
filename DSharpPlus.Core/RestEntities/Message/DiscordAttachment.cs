using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <remarks>
    /// For the attachments array in Message Create/Edit requests, only the id is required.
    /// </remarks>
    public sealed record DiscordAttachment
    {
        /// <summary>
        /// The attachment id.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The name of the file attached.
        /// </summary>
        [JsonProperty("filename", NullValueHandling = NullValueHandling.Ignore)]
        public string Filename { get; init; } = null!;

        /// <summary>
        /// The description for the file.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Description { get; init; }

        /// <summary>
        /// The attachment's <see href="https://en.wikipedia.org/wiki/Media_type">media type</see>.
        /// </summary>
        [JsonProperty("content_type", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> ContentType { get; init; }

        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public int Size { get; init; }

        /// <summary>
        /// The source url of file.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; init; } = null!;

        /// <summary>
        /// A proxied url of file.
        /// </summary>
        [JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
        public string ProxyUrl { get; init; } = null!;

        /// <summary>
        /// The height of file (if image).
        /// </summary>
        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int?> Height { get; init; }

        /// <summary>
        /// The width of file (if image).
        /// </summary>
        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int?> Width { get; init; }

        /// <summary>
        /// Whether this attachment is ephemeral.
        /// </summary>
        /// <remarks>
        /// Ephemeral attachments will automatically be removed after a set period of time. Ephemeral attachments on messages are guaranteed to be available as long as the message itself exists.
        /// </remarks>
        [JsonProperty("ephemeral", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Ephemeral { get; init; }
    }
}
