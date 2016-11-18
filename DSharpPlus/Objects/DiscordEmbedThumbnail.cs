using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordEmbedThumbnail
    {
        /// <summary>
        /// Source url of the thumbnail (only https)
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
        /// <summary>
        /// A proxied url of the thumbnail
        /// </summary>
        [JsonProperty("proxy_url")]
        public string ProxyUrl { get; set; }
        /// <summary>
        /// Height of the thumbnail
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; set; }
        /// <summary>
        /// Width of the thumbnail
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }
    }
}
