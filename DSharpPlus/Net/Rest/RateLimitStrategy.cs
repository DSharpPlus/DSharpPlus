namespace DSharpPlus.Net;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Polly;

internal class RateLimitStrategy : ResilienceStrategy<HttpResponseMessage>, IDisposable
{
    private readonly RateLimitBucket globalBucket;
    private readonly ConcurrentDictionary<string, RateLimitBucket> buckets = [];
    private readonly ConcurrentDictionary<string, string> routeHashes = [];

    private readonly ILogger logger;
    private readonly int waitingForHashMilliseconds;

    private bool cancel = false;

    public RateLimitStrategy(ILogger logger, int waitingForHashMilliseconds = 200, int maximumRestRequestsPerSecond = 15)
    {
        this.logger = logger;
        this.waitingForHashMilliseconds = waitingForHashMilliseconds;

        globalBucket = new(maximumRestRequestsPerSecond, maximumRestRequestsPerSecond, DateTime.UtcNow.AddSeconds(1));

        _ = CleanAsync();
    }

    protected override async ValueTask<Outcome<HttpResponseMessage>> ExecuteCore<TState>
    (
        Func<ResilienceContext, TState, ValueTask<Outcome<HttpResponseMessage>>> action,
        ResilienceContext context,
        TState state
    )
    {
        // fail-fast if we dont have a route to ratelimit to
#pragma warning disable CS8600
        if (!context.Properties.TryGetValue(new("route"), out string route))
        {
            return Outcome.FromException<HttpResponseMessage>(
                new InvalidOperationException("No route passed. This should be reported to library developers."));
        }
#pragma warning restore CS8600

        // get trace id for logging
        Ulid traceId = context.Properties.TryGetValue(new("trace-id"), out Ulid tid) ? tid : Ulid.Empty;

        // get global limit
        bool exemptFromGlobalLimit = false;

        if (context.Properties.TryGetValue(new("exempt-from-global-limit"), out bool exempt))
        {
            exemptFromGlobalLimit = exempt;
        }

        // check against ratelimits now
        DateTime instant = DateTime.UtcNow;

        if (!exemptFromGlobalLimit && !globalBucket.CheckNextRequest())
        {
            return SynthesizeInternalResponse(route, globalBucket.Reset, "global", traceId);
        }

        if (!routeHashes.TryGetValue(route, out string? hash))
        {
            logger.LogTrace
            (
                LoggerEvents.RatelimitDiag,
                "Request ID:{TraceId}: Route has no known hash: {Route}.",
                traceId,
                route
            );

            routeHashes.AddOrUpdate(route, "pending", (_, _) => "pending");

            Outcome<HttpResponseMessage> outcome = await action(context, state);

            if (!exemptFromGlobalLimit)
            {
                globalBucket.CompleteReservation();
            }

            if (outcome.Result is null)
            {
                routeHashes.Remove(route, out _);
                return outcome;
            }

            UpdateRateLimitBuckets(outcome.Result, "pending", route, traceId);

            // something went awry, just reset and try again next time. this may be because the endpoint didn't return valid headers,
            // which is the case for some endpoints, and we don't need to get hung up on this
            if (routeHashes[route] == "pending")
            {
                routeHashes.Remove(route, out _);
            }

            return outcome;
        }
        else if (hash == "pending")
        {
            if (!exemptFromGlobalLimit)
            {
                globalBucket.CancelReservation();
            }

            return SynthesizeInternalResponse
            (
                route,
                instant + TimeSpan.FromMilliseconds(waitingForHashMilliseconds),
                "route",
                traceId
            );
        }
        else
        {
            RateLimitBucket bucket = buckets.GetOrAdd(hash, _ => new());

            logger.LogTrace
            (
                LoggerEvents.RatelimitDiag,
                "Request ID:{TraceId}: Checking bucket, current state is [Remaining: {Remaining}, Reserved: {Reserved}]",
                traceId,
                bucket.remaining,
                bucket.reserved
            );

            if (!bucket.CheckNextRequest())
            {
                if (!exemptFromGlobalLimit)
                {
                    globalBucket.CancelReservation();
                }

                return SynthesizeInternalResponse(route, bucket.Reset, "bucket", traceId);
            }

            logger.LogTrace
            (
                LoggerEvents.RatelimitDiag,
                "Request ID:{TraceId}: Allowed request, current state is [Remaining: {Remaining}, Reserved: {Reserved}]",
                traceId,
                bucket.remaining,
                bucket.reserved
            );

            Outcome<HttpResponseMessage> outcome;

            try
            {
                // make the actual request
                outcome = await action(context, state);

                if (outcome.Result is null)
                {
                    if (!exemptFromGlobalLimit)
                    {
                        globalBucket.CancelReservation();
                    }

                    return outcome;
                }

                if (!exemptFromGlobalLimit)
                {
                    globalBucket.CompleteReservation();
                }
            }
            catch (Exception e)
            {
                if (!exemptFromGlobalLimit)
                {
                    globalBucket.CancelReservation();
                }

                bucket.CancelReservation();
                return Outcome.FromException<HttpResponseMessage>(e);
            }

            if (!exemptFromGlobalLimit)
            {
                UpdateRateLimitBuckets(outcome.Result, hash, route, traceId);
            }

            return outcome;
        }
    }

    private Outcome<HttpResponseMessage> SynthesizeInternalResponse(string route, DateTime retry, string scope, Ulid traceId)
    {
        string waitingForRoute = scope == "route" ? " for route hash" : "";
        string global = scope == "global" ? " global" : "";

        string traceIdString = "";
        if (logger.IsEnabled(LogLevel.Trace))
        {
            traceIdString = $"Request ID:{traceId}: ";
        }

        DateTime retryJittered = retry + TimeSpan.FromMilliseconds(Random.Shared.NextInt64(100));

        logger.LogDebug
        (
            LoggerEvents.RatelimitPreemptive,
            "{TraceId}Pre-emptive{Global} ratelimit for {Route} triggered - waiting{WaitingForRoute} until {Reset:O}.",
            traceIdString,
            global,
            route,
            waitingForRoute,
            retryJittered
        );

        return Outcome.FromException<HttpResponseMessage>(
            new PreemptiveRatelimitException(scope, retryJittered - DateTime.UtcNow));
    }

    private void UpdateRateLimitBuckets(HttpResponseMessage response, string oldHash, string route, Ulid id)
    {
        if (response.Headers.TryGetValues("X-RateLimit-Bucket", out IEnumerable<string>? hashHeader))
        {
            string newHash = hashHeader?.Single()!;

            if (!RateLimitBucket.TryExtractRateLimitBucket(response.Headers, out RateLimitCandidateBucket extracted))
            {
                return;
            }
            else if (oldHash != newHash)
            {
                logger.LogTrace("Request ID:{ID} - Initial bucket capacity: {max}", id, extracted.Maximum);
                buckets.AddOrUpdate(newHash, _ => extracted.ToFullBucket(), (_, _) => extracted.ToFullBucket());
            }
            else
            {
                if (buckets.TryGetValue(newHash, out RateLimitBucket? oldBucket))
                {
                    oldBucket.UpdateBucket(extracted.Maximum, extracted.Remaining, extracted.Reset);
                }
                else
                {
                    logger.LogTrace("Request ID:{ID} - Initial bucket capacity: {max}", id, extracted.Maximum);
                    buckets.AddOrUpdate(newHash, _ => extracted.ToFullBucket(),
                        (_, _) => extracted.ToFullBucket());
                }
            }

            routeHashes.AddOrUpdate(route, newHash!, (_, _) => newHash!);
        }
    }

    private async ValueTask CleanAsync()
    {
        PeriodicTimer timer = new(TimeSpan.FromSeconds(10));

        while (await timer.WaitForNextTickAsync())
        {
            foreach (KeyValuePair<string, string> pair in routeHashes)
            {
                if (buckets.TryGetValue(pair.Value, out RateLimitBucket? bucket) && bucket.Reset < DateTime.UtcNow + TimeSpan.FromSeconds(1))
                {
                    buckets.Remove(pair.Value, out _);
                    routeHashes.Remove(pair.Key, out _);
                }
            }

            if (cancel)
            {
                return;
            }
        }
    }

    public void Dispose() => cancel = true;
}
