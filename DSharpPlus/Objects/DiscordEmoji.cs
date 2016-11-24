using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Objects
{
    class DiscordEmoji
    {
        [JsonProperty("id")]
        public string ID { get; internal set; }

        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("roles")]
        public List<DiscordRole> Roles { get; internal set; }

        [JsonProperty("require_colons")]
        public bool RequireColons { get; internal set; }

        [JsonProperty("managed")]
        public bool IsManaged { get; internal set; }
    }
}
