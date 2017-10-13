using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// Gets the headers sent with this request.
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; }

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
        /// <param name="headers">Additional headers for this request.</param>
        /// <param name="ratelimitWaitOverride">Override for ratelimit bucket wait time.</param>
        internal BaseRestRequest(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, IDictionary<string, string> headers = null, double? ratelimitWaitOverride = null)
        {
            Discord = client;
            RateLimitBucket = bucket;
            RequestTaskSource = new TaskCompletionSource<RestResponse>();
            Url = url;
            Method = method;
            Headers = headers != null ? new ReadOnlyDictionary<string, string>(headers) : null;
            RateLimitWaitOverride = ratelimitWaitOverride;
        }

        /// <summary>
        /// Asynchronously waits for this request to complete.
        /// </summary>
        /// <returns>HTTP response to this request.</returns>
        public Task<RestResponse> WaitForCompletionAsync() =>
            RequestTaskSource.Task;

        protected internal void SetCompleted(RestResponse response) =>
            RequestTaskSource.SetResult(response);

        protected internal void SetFaulted(Exception ex) =>
            RequestTaskSource.SetException(ex);
    }
}
