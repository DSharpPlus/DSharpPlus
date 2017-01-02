using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordApplicationOwner : SnowflakeObject
    {
        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username;

        [JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
        public string Discriminator;

        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public string Avatar;
    }
}
