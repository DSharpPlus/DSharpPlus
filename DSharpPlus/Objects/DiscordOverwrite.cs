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
        [JsonProperty("type")]
        public string Type { get; internal set; }
        /// <summary>
        /// Termission bit set
        /// </summary>
        [JsonProperty("allow")]
        public int Allow { get; internal set; }
        /// <summary>
        /// Permission bit set
        /// </summary>
        [JsonProperty("deny")]
        public int Deny { get; internal set; }
    }
}
