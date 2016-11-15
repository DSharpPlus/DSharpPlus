using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordOverwrite : SnowflakeObject
    {
        [JsonProperty("type")]
        public string Type { get; internal set; }
        [JsonProperty("allow")]
        public int Allow { get; internal set; }
        [JsonProperty("deny")]
        public int Deny { get; internal set; }
    }
}
