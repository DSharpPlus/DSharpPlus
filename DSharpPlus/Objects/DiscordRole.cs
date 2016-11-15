using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordRole : SnowflakeObject
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("color")]
        public int Color { get; internal set; }
        [JsonProperty("hoist")]
        public bool Hoist { get; internal set; }
        [JsonProperty("position")]
        public int Position { get; internal set; }
        [JsonProperty("permissions")]
        public int Permissions { get; internal set; }
        [JsonProperty("managed")]
        public bool Managed { get; internal set; }
        [JsonProperty("mentionable")]
        public bool Mentionable { get; internal set; }
    }
}
