using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordEmbedField
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("value")]
        public string Value { get; internal set; }
        [JsonProperty("inline")]
        public bool Inline { get; internal set; }
    }
}
