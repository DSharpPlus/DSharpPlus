using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordEmbedFooter
    {
        /// <summary>
        /// Footer text
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }
        /// <summary>
        /// Url of the footer icon (https only)
        /// </summary>
        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }
        /// <summary>
        /// A proxied url of the footer icon
        /// </summary>
        [JsonProperty("proxy_icon_url")]
        public string ProxyIconUrl { get; set; }
    }
}
