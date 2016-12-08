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
<<<<<<< HEAD
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("name")]
>>>>>>> master
        public string Name { get; set; }
        /// <summary>
        /// Url of the author
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("url")]
>>>>>>> master
        public string Url { get; set; }
        /// <summary>
        /// Url of the author icon (https only)
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("icon_url", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("icon_url")]
>>>>>>> master
        public string IconUrl { get; set; }
        /// <summary>
        /// A proxied url of the author icon
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("proxy_icon_url", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("proxy_icon_url")]
>>>>>>> master
        public string ProxyIconUrl { get; set; }
    }
}
