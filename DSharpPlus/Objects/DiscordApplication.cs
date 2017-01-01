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
        [JsonProperty("description")]
        public string Description;

        [JsonProperty("icon")]
        public string Icon;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("rpc_origins")]
        public List<string> RpcOrigins;

        [JsonProperty("flags")]
        public int Flags;

        [JsonProperty("owner")]
        public DiscordApplicationOwner Owner;
    }
}
