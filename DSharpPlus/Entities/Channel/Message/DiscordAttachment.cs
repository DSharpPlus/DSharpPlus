using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents an attachment for a message.
    /// </summary>
    public class DiscordAttachment : SnowflakeObject
    {
        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        [JsonProperty("filename", NullValueHandling = NullValueHandling.Ignore)]
        public string FileName { get; internal set; }

        /// <summary>
        /// Gets the file size in bytes.
        /// </summary>
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public int FileSize { get; internal set; }

        /// <summary>
        /// Gets the media, or MIME, type of the file.
        /// </summary>
        [JsonProperty("content_type", NullValueHandling = NullValueHandling.Ignore)]
        public string MediaType { get; internal set; }

        /// <summary>
        /// Gets the URL of the file.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; internal set; }

        /// <summary>
        /// Gets the proxied URL of the file.
        /// </summary>
        [JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
        public string ProxyUrl { get; internal set; }

        /// <summary>
        /// Gets the height. Applicable only if the attachment is an image.
        /// </summary>
        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public int? Height { get; internal set; }

        /// <summary>
        /// Gets the width. Applicable only if the attachment is an image.
        /// </summary>
        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public int? Width { get; internal set; }

        /// <summary>
        /// Gets whether this attachment is ephemeral.
        /// </summary>
        [JsonProperty("ephemeral", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Ephemeral { get; internal set; }

        internal DiscordAttachment() { }
    }
}
