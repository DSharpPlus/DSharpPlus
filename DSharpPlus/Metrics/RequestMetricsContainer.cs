using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;

namespace DSharpPlus.Metrics;

internal sealed class RequestMetricsContainer
{
    private LiveRequestMetrics lifetime = default;
    private LiveRequestMetrics temporal = default;
    private DateTimeOffset lastReset = DateTimeOffset.UtcNow;
    private readonly DateTimeOffset creation = DateTimeOffset.UtcNow;

    public RequestMetricsCollection GetLifetimeMetrics()
    {
        return new()
        {
            Duration = DateTimeOffset.UtcNow - this.creation,

            BadRequests = this.lifetime.badRequests,
            BucketRatelimitsHit = this.lifetime.bucketRatelimits,
            Forbidden = this.lifetime.forbidden,
            GlobalRatelimitsHit = this.lifetime.globalRatelimits,
            NotFound = this.lifetime.notFound,
            RatelimitsHit = this.lifetime.ratelimits,
            ServerErrors = this.lifetime.serverError,
            SuccessfulRequests = this.lifetime.successful,
            TooLarge = this.lifetime.tooLarge,
            TotalRequests = this.lifetime.requests
        };
    }

    public RequestMetricsCollection GetTemporalMetrics()
    {
        RequestMetricsCollection collection = new()
        {
            Duration = DateTimeOffset.UtcNow - this.lastReset,

            BadRequests = this.temporal.badRequests,
            BucketRatelimitsHit = this.temporal.bucketRatelimits,
            Forbidden = this.temporal.forbidden,
            GlobalRatelimitsHit = this.temporal.globalRatelimits,
            NotFound = this.temporal.notFound,
            RatelimitsHit = this.temporal.ratelimits,
            ServerErrors = this.temporal.serverError,
            SuccessfulRequests = this.temporal.successful,
            TooLarge = this.temporal.tooLarge,
            TotalRequests = this.temporal.requests
        };

        this.lastReset = DateTimeOffset.UtcNow;
        this.temporal = default;

        return collection;
    }

    public void RegisterBadRequest()
    {
        Interlocked.Increment(ref this.lifetime.badRequests);
        Interlocked.Increment(ref this.temporal.badRequests);

        Interlocked.Increment(ref this.lifetime.requests);
        Interlocked.Increment(ref this.temporal.requests);
    }

    public void RegisterForbidden()
    {
        Interlocked.Increment(ref this.lifetime.forbidden);
        Interlocked.Increment(ref this.temporal.forbidden);

        Interlocked.Increment(ref this.lifetime.requests);
        Interlocked.Increment(ref this.temporal.requests);
    }

    public void RegisterNotFound()
    {
        Interlocked.Increment(ref this.lifetime.notFound);
        Interlocked.Increment(ref this.temporal.notFound);

        Interlocked.Increment(ref this.lifetime.requests);
        Interlocked.Increment(ref this.temporal.requests);
    }

    public void RegisterRequestTooLarge()
    {
        Interlocked.Increment(ref this.lifetime.tooLarge);
        Interlocked.Increment(ref this.temporal.tooLarge);

        Interlocked.Increment(ref this.lifetime.requests);
        Interlocked.Increment(ref this.temporal.requests);
    }

    public void RegisterRatelimitHit(HttpResponseHeaders headers)
    {
        if (headers.TryGetValues("x-ratelimit-scope", out IEnumerable<string>? values) && values.First() == "global")
        {
            Interlocked.Increment(ref this.lifetime.globalRatelimits);
            Interlocked.Increment(ref this.temporal.globalRatelimits);
        }
        else
        {
            Interlocked.Increment(ref this.lifetime.bucketRatelimits);
            Interlocked.Increment(ref this.temporal.bucketRatelimits);
        }

        Interlocked.Increment(ref this.lifetime.ratelimits);
        Interlocked.Increment(ref this.temporal.ratelimits);

        Interlocked.Increment(ref this.lifetime.requests);
        Interlocked.Increment(ref this.temporal.requests);
    }

    public void RegisterServerError()
    {
        Interlocked.Increment(ref this.lifetime.serverError);
        Interlocked.Increment(ref this.temporal.serverError);

        Interlocked.Increment(ref this.lifetime.requests);
        Interlocked.Increment(ref this.temporal.requests);
    }

    public void RegisterSuccess()
    {
        Interlocked.Increment(ref this.lifetime.successful);
        Interlocked.Increment(ref this.temporal.successful);

        Interlocked.Increment(ref this.lifetime.requests);
        Interlocked.Increment(ref this.temporal.requests);
    }
}
