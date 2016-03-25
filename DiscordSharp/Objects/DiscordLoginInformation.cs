using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Objects
{
    [Obsolete]
    public class DiscordLoginInformation
    {
        [JsonProperty("email")]
        public string[] Email { get; set; }
        [JsonProperty("password")]
        public string[] Password { get; set; }

        public string AsJson()
        {
            return JsonConvert.SerializeObject(this);
        }
        public DiscordLoginInformation()
        {
            Email = new string[1];
            Password = new string[1];
        }
    }
}
