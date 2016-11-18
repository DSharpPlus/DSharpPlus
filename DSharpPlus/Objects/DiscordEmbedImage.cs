using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordEmbedImage
    {
        /// <summary>
        /// Source url of the image (https only)
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
        /// <summary>
        /// A proxied url of the image
        /// </summary>
        [JsonProperty("proxy_url")]
        public string ProxyUrl { get; set; }
        /// <summary>
        /// Height of the image
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; set; }
        /// <summary>
        /// Width of the image
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }
    }
}
