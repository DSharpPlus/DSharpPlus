using Newtonsoft.Json;

namespace DSharpPlus.Rest
{
    public class AuthorizationCodeGrantResponse : BaseTokenResponse
    {
        [JsonProperty("refresh_token")]
        public string RefreshToken;
    }
}