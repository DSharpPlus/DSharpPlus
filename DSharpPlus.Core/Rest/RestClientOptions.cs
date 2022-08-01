using Microsoft.Extensions.Options;

namespace DSharpPlus.Core.Rest
{
    /// <summary>
    /// Stores options for the D#+ Rest Client.
    /// </summary>
    public class RestClientOptions : IOptions<RestClientOptions>
    {
        public RestClientOptions Value => this;

        /// <summary>
        /// The authorization token used for the rest client.
        /// </summary>
        public string Token { get; set; } = null!;

        /// <summary>
        /// The amount of retries the rest client will attempt before dropping the request.
        /// </summary>
        public int MaximumRetries { get; set; }
    }
}
