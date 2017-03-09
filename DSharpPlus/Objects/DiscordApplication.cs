using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class DiscordApplication : SnowflakeObject
    {
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; internal set; }

        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string Icon { get; internal set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        [JsonProperty("rpc_origins", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyCollection<string> RpcOrigins { get; internal set; }

        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public int Flags { get; internal set; }

        [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser Owner { get; internal set; }
    }
}
