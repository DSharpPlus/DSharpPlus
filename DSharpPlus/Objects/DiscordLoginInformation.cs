using Newtonsoft.Json;
using System;

namespace DSharpPlus.Objects
{
    /// <summary>
    /// Login information for user accounts
    /// </summary>
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
