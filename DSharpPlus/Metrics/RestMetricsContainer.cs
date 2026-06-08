using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;

namespace DSharpPlus.Metrics;

/// <summary>
/// Provides a mechanism to obtain metrics about the success and failures of REST calls.
/// </summary>
public sealed class RestMetricsContainer
{
    private LiveRequestMetrics metrics;
    private readonly DateTimeOffset creation = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets a snapshot of metrics as collected so far.
    /// </summary>
    public RestMetricsCollection GetCollectedMetrics()
    {
        return new()
        {
            Duration = DateTimeOffset.UtcNow - this.creation,

            BadRequests = this.metrics.badRequests,
            BucketRatelimitsHit = this.metrics.bucketRatelimits,
            Forbidden = this.metrics.forbidden,
            GlobalRatelimitsHit = this.metrics.globalRatelimits,
            NotFound = this.metrics.notFound,
            RatelimitsHit = this.metrics.ratelimits,
            ServerErrors = this.metrics.serverError,
            SuccessfulRequests = this.metrics.successful,
            TooLarge = this.metrics.tooLarge,
            TotalRequests = this.metrics.requests
        };
    }

    internal void RegisterRestRequestOutcome(HttpStatusCode statusCode, HttpResponseHeaders headers)
    {
        Interlocked.Increment(ref this.metrics.requests);

        if ((int)statusCode is >= 400 and <= 599)
        {
            if (statusCode == HttpStatusCode.TooManyRequests && headers.TryGetValues("x-ratelimit-scope", out IEnumerable<string>? values))
            {
                GlobalMeter.RecordFailedRestRequest(statusCode, values.First());
                
                if (values.First() is "global" or "shared")
                {
                    Interlocked.Increment(ref this.metrics.globalRatelimits);
                }
                else
                {
                    Interlocked.Increment(ref this.metrics.bucketRatelimits);
                }
            }
            else
            {
                GlobalMeter.RecordFailedRestRequest(statusCode);
            }
        }
        else
        {
            GlobalMeter.RecordSuccessfulRestRequest();
            Interlocked.Increment(ref this.metrics.successful);
        }

        switch (statusCode)
        {
            case HttpStatusCode.BadRequest or HttpStatusCode.MethodNotAllowed:

                Interlocked.Increment(ref this.metrics.badRequests);
                break;

            case HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden:

                Interlocked.Increment(ref this.metrics.forbidden);
                break;

            case HttpStatusCode.NotFound:

                Interlocked.Increment(ref this.metrics.notFound);
                break;

            case HttpStatusCode.RequestEntityTooLarge:

                Interlocked.Increment(ref this.metrics.tooLarge);
                break;

            case HttpStatusCode.TooManyRequests:

                Interlocked.Increment(ref this.metrics.ratelimits);
                break;

            case HttpStatusCode.InternalServerError
                or HttpStatusCode.BadGateway
                or HttpStatusCode.ServiceUnavailable
                or HttpStatusCode.GatewayTimeout:

                Interlocked.Increment(ref this.metrics.serverError);
                break;
        }
    }

    private struct LiveRequestMetrics
    {
        public ulong requests;
        public ulong successful;
        public int ratelimits;
        public int globalRatelimits;
        public int bucketRatelimits;
        public int badRequests;
        public int forbidden;
        public int notFound;
        public int tooLarge;
        public int serverError;
    }
}
