using Newtonsoft.Json;

namespace DSharpPlus.Rest
{
	public class RefreshTokenResponse : BaseTokenResponse
	{
		[JsonProperty("refresh_token")]
		public string RefreshToken;
	}
}