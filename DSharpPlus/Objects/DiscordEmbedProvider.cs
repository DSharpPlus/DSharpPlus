using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordEmbedProvider
    {
        /// <summary>
        /// Name of the provider
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// Url of the provider
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
