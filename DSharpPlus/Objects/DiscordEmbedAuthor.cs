using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordEmbedAuthor
    {
        /// <summary>
        /// Name of the author
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// Url of the author
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
        /// <summary>
        /// Url of the author icon (https only)
        /// </summary>
        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }
        /// <summary>
        /// A proxied url of the author icon
        /// </summary>
        [JsonProperty("proxy_icon_url")]
        public string ProxyIconUrl { get; set; }
    }
}
