using Newtonsoft.Json;

namespace DSharpPlus.OAuth2
{
	public class AuthorizationCodeGrantResponse : BaseTokenResponse
	{
		[JsonProperty("refresh_token")]
		public string RefreshToken;
	}
}