using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordOverwrite : SnowflakeObject
    {
        // TODO -> Enum for that?
        /// <summary>
        /// Either "role" or "member"
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; internal set; }
        /// <summary>
        /// Termission bit set
        /// </summary>
        [JsonProperty("allow", NullValueHandling = NullValueHandling.Ignore)]
        public int Allow { get; internal set; }
        /// <summary>
        /// Permission bit set
        /// </summary>
        [JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
        public int Deny { get; internal set; }
    }
}
