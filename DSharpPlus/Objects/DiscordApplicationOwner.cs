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
        [JsonProperty("username")]
        public string Username;

        [JsonProperty("discriminator")]
        public string Discriminator;

        [JsonProperty("avatar")]
        public string Avatar;
    }
}
