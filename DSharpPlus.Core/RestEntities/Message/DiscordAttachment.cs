using System.Text.Json.Serialization;

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
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The name of the file attached.
        /// </summary>
        [JsonPropertyName("filename")]
        public string Filename { get; init; } = null!;

        /// <summary>
        /// The description for the file.
        /// </summary>
        [JsonPropertyName("description")]
        public Optional<string> Description { get; init; }

        /// <summary>
        /// The attachment's <see href="https://en.wikipedia.org/wiki/Media_type">media type</see>.
        /// </summary>
        [JsonPropertyName("content_type")]
        public Optional<string> ContentType { get; init; }

        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        [JsonPropertyName("size")]
        public int Size { get; init; }

        /// <summary>
        /// The source url of file.
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; init; } = null!;

        /// <summary>
        /// A proxied url of file.
        /// </summary>
        [JsonPropertyName("proxy_url")]
        public string ProxyUrl { get; init; } = null!;

        /// <summary>
        /// The height of file (if image).
        /// </summary>
        [JsonPropertyName("height")]
        public Optional<int?> Height { get; init; }

        /// <summary>
        /// The width of file (if image).
        /// </summary>
        [JsonPropertyName("width")]
        public Optional<int?> Width { get; init; }

        /// <summary>
        /// Whether this attachment is ephemeral.
        /// </summary>
        /// <remarks>
        /// Ephemeral attachments will automatically be removed after a set period of time. Ephemeral attachments on messages are guaranteed to be available as long as the message itself exists.
        /// </remarks>
        [JsonPropertyName("ephemeral")]
        public Optional<bool> Ephemeral { get; init; }
    }
}
