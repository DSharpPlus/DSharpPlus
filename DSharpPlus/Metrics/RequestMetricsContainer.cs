namespace DSharpPlus.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;

internal sealed class RequestMetricsContainer
{
    private LiveRequestMetrics lifetime = default;
    private LiveRequestMetrics temporal = default;
    private DateTimeOffset lastReset = DateTimeOffset.UtcNow;
    private readonly DateTimeOffset creation = DateTimeOffset.UtcNow;

    public RequestMetricsCollection GetLifetimeMetrics() => new()
    {
        Duration = DateTimeOffset.UtcNow - creation,

        BadRequests = lifetime.badRequests,
        BucketRatelimitsHit = lifetime.bucketRatelimits,
        Forbidden = lifetime.forbidden,
        GlobalRatelimitsHit = lifetime.globalRatelimits,
        NotFound = lifetime.notFound,
        RatelimitsHit = lifetime.ratelimits,
        ServerErrors = lifetime.serverError,
        SuccessfulRequests = lifetime.successful,
        TooLarge = lifetime.tooLarge,
        TotalRequests = lifetime.requests
    };

    public RequestMetricsCollection GetTemporalMetrics()
    {
        RequestMetricsCollection collection = new()
        {
            Duration = DateTimeOffset.UtcNow - lastReset,

            BadRequests = temporal.badRequests,
            BucketRatelimitsHit = temporal.bucketRatelimits,
            Forbidden = temporal.forbidden,
            GlobalRatelimitsHit = temporal.globalRatelimits,
            NotFound = temporal.notFound,
            RatelimitsHit = temporal.ratelimits,
            ServerErrors = temporal.serverError,
            SuccessfulRequests = temporal.successful,
            TooLarge = temporal.tooLarge,
            TotalRequests = temporal.requests
        };

        lastReset = DateTimeOffset.UtcNow;
        temporal = default;

        return collection;
    }

    public void RegisterBadRequest()
    {
        Interlocked.Increment(ref lifetime.badRequests);
        Interlocked.Increment(ref temporal.badRequests);

        Interlocked.Increment(ref lifetime.requests);
        Interlocked.Increment(ref temporal.requests);
    }

    public void RegisterForbidden()
    {
        Interlocked.Increment(ref lifetime.forbidden);
        Interlocked.Increment(ref temporal.forbidden);

        Interlocked.Increment(ref lifetime.requests);
        Interlocked.Increment(ref temporal.requests);
    }

    public void RegisterNotFound()
    {
        Interlocked.Increment(ref lifetime.notFound);
        Interlocked.Increment(ref temporal.notFound);

        Interlocked.Increment(ref lifetime.requests);
        Interlocked.Increment(ref temporal.requests);
    }

    public void RegisterRequestTooLarge()
    {
        Interlocked.Increment(ref lifetime.tooLarge);
        Interlocked.Increment(ref temporal.tooLarge);

        Interlocked.Increment(ref lifetime.requests);
        Interlocked.Increment(ref temporal.requests);
    }

    public void RegisterRatelimitHit(HttpResponseHeaders headers)
    {
        if (headers.TryGetValues("x-ratelimit-scope", out IEnumerable<string>? values) && values.First() == "global")
        {
            Interlocked.Increment(ref lifetime.globalRatelimits);
            Interlocked.Increment(ref temporal.globalRatelimits);
        }
        else
        {
            Interlocked.Increment(ref lifetime.bucketRatelimits);
            Interlocked.Increment(ref temporal.bucketRatelimits);
        }

        Interlocked.Increment(ref lifetime.ratelimits);
        Interlocked.Increment(ref temporal.ratelimits);

        Interlocked.Increment(ref lifetime.requests);
        Interlocked.Increment(ref temporal.requests);
    }

    public void RegisterServerError()
    {
        Interlocked.Increment(ref lifetime.serverError);
        Interlocked.Increment(ref temporal.serverError);

        Interlocked.Increment(ref lifetime.requests);
        Interlocked.Increment(ref temporal.requests);
    }

    public void RegisterSuccess()
    {
        Interlocked.Increment(ref lifetime.successful);
        Interlocked.Increment(ref temporal.successful);

        Interlocked.Increment(ref lifetime.requests);
        Interlocked.Increment(ref temporal.requests);
    }
}
