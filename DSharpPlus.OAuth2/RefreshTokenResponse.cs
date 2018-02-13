using Newtonsoft.Json;

namespace DSharpPlus.OAuth2
{
	public class RefreshTokenResponse : BaseTokenResponse
	{
		[JsonProperty("refresh_token")]
		public string RefreshToken;
	}
}