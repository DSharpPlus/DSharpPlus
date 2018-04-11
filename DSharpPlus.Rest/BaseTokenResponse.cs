using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Rest
{
    public class BaseTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("token_type")]
        public TokenType TokenType { get; set; }
        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }
        [JsonIgnore]
        public Scope[] Scopes { get; set; }
        [JsonProperty("scope")]
        internal string scope;


        public virtual DiscordConfiguration GetDiscordConfiguration()
        {
            return new DiscordConfiguration()
            {
                Token = AccessToken,
                TokenType = TokenType
            };
        }
    }
}
