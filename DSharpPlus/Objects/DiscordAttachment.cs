using Newtonsoft.Json;

namespace DSharpPlus.Objects
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordAttachment : SnowflakeObject
    {
        /// <summary>
        /// Name of the file
        /// </summary>
        [JsonProperty("filename", NullValueHandling = NullValueHandling.Ignore)]
        public string FileName { get; internal set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public int FileSize { get; internal set; }

        /// <summary>
        /// File URL
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; internal set; }

        /// <summary>
        /// Proxy file URL
        /// </summary>
        [JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
        public string ProxyUrl { get; internal set; }

        /// <summary>
        /// Height (if image)
        /// </summary>
        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public int Height { get; internal set; }

        /// <summary>
        /// Width (if image)
        /// </summary>
        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public int Width { get; internal set; }
    }
}
