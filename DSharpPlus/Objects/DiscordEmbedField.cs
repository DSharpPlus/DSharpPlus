using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordEmbedField
    {
        /// <summary>
        /// Name of the field
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        /// <summary>
        /// Value of the field
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
        /// <summary>
        /// Whether or not this field should display inline
        /// </summary>
        [JsonProperty("inline", NullValueHandling = NullValueHandling.Ignore)]
        public bool Inline { get; set; }
    }
}
