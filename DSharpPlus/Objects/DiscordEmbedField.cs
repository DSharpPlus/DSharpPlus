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
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// Value of the field
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
        /// <summary>
        /// Whether or not this field should display inline
        /// </summary>
        [JsonProperty("inline")]
        public bool Inline { get; set; }
    }
}
