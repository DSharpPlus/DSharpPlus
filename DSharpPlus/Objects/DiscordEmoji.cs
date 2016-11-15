using Newtonsoft.Json;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class DiscordEmoji : SnowflakeObject
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("roles")]
        public List<DiscordRole> Roles { get; internal set; }
        [JsonProperty("require_colons")]
        public bool RequireColons { get; internal set; }
        [JsonProperty("managed")]
        public bool Managed { get; internal set; }
    }
}
