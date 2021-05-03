using System;
using System.Collections.Generic;

namespace DSharpPlus.Net
{
    /// <summary>
    /// Represents a non-multipart HTTP request.
    /// </summary>
    internal sealed class RestRequest : BaseRestRequest
    {
        /// <summary>
        /// Gets the payload sent with this request.
        /// </summary>
        public string Payload { get; }

        internal RestRequest(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null, string payload = null, double? ratelimitWaitOverride = null)
            : base(client, bucket, url, method, route, headers, ratelimitWaitOverride)
        {
            this.Payload = payload;
        }
    }
}
