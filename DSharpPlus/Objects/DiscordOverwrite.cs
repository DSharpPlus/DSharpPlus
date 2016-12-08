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
<<<<<<< HEAD
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("type")]
>>>>>>> master
        public string Type { get; internal set; }
        /// <summary>
        /// Termission bit set
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("allow", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("allow")]
>>>>>>> master
        public int Allow { get; internal set; }
        /// <summary>
        /// Permission bit set
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("deny")]
>>>>>>> master
        public int Deny { get; internal set; }
    }
}
