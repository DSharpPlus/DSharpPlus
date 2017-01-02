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
        public string Description;

        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string Icon;

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name;

        [JsonProperty("rpc_origins", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> RpcOrigins;

        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public int Flags;

        [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordApplicationOwner Owner;
    }
}
