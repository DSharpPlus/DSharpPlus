using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordIntegrationAccount : SnowflakeObject
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }
    }
}
