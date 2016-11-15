using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class DiscordInviteChannel : SnowflakeObject
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("type")]
        public ChannelType Type { get; internal set; }
    }
}
