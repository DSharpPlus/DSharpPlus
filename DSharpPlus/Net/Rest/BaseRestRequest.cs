using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.Net
{
    /// <summary>
    /// Represents a request sent over HTTP.
    /// </summary>
    public abstract class BaseRestRequest
    {
        protected internal BaseDiscordClient Discord { get; }
        protected internal TaskCompletionSource<RestResponse> RequestTaskSource { get; }

        /// <summary>
        /// Gets the url to which this request is going to be made.
        /// </summary>
        public Uri Url { get; }

        /// <summary>
        /// Gets the HTTP method used for this request.
        /// </summary>
        public RestRequestMethod Method { get; }

        /// <summary>
        /// Gets the generic path (no parameters) for this request.
        /// </summary>
        public string Route { get; }

        /// <summary>
        /// Gets the headers sent with this request.
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; } = null;

        /// <summary>
        /// Gets the override for the rate limit bucket wait time.
        /// </summary>
        public double? RateLimitWaitOverride { get; }

        /// <summary>
        /// Gets the rate limit bucket this request is in.
        /// </summary>
        internal RateLimitBucket RateLimitBucket { get; }

        /// <summary>
        /// Creates a new <see cref="BaseRestRequest"/> with specified parameters.
        /// </summary>
        /// <param name="client"><see cref="DiscordClient"/> from which this request originated.</param>
        /// <param name="bucket">Rate limit bucket to place this request in.</param>
        /// <param name="url">Uri to which this request is going to be sent to.</param>
        /// <param name="method">Method to use for this request,</param>
        /// <param name="route">The generic route the request url will use.</param>
        /// <param name="headers">Additional headers for this request.</param>
        /// <param name="ratelimitWaitOverride">Override for ratelimit bucket wait time.</param>
        internal BaseRestRequest(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null, double? ratelimitWaitOverride = null)
        {
            this.Discord = client;
            this.RateLimitBucket = bucket;
            this.RequestTaskSource = new TaskCompletionSource<RestResponse>();
            this.Url = url;
            this.Method = method;
            this.Route = route;
            this.RateLimitWaitOverride = ratelimitWaitOverride;

            if (headers != null)
            {
                headers = headers.Select(x => new KeyValuePair<string, string>(x.Key, Uri.EscapeDataString(x.Value)))
                    .ToDictionary(x => x.Key, x => x.Value);
                this.Headers = headers;
            }
        }

        /// <summary>
        /// Asynchronously waits for this request to complete.
        /// </summary>
        /// <returns>HTTP response to this request.</returns>
        public Task<RestResponse> WaitForCompletionAsync()
            => this.RequestTaskSource.Task;

        protected internal void SetCompleted(RestResponse response)
            => this.RequestTaskSource.SetResult(response);

        protected internal void SetFaulted(Exception ex)
            => this.RequestTaskSource.SetException(ex);

        protected internal bool TrySetFaulted(Exception ex)
            => this.RequestTaskSource.TrySetException(ex);

    }
}
