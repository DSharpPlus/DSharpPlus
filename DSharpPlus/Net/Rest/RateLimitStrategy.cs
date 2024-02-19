using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Polly;

namespace DSharpPlus.Net;

internal class RateLimitStrategy : ResilienceStrategy<HttpResponseMessage>, IDisposable
{
    private readonly RateLimitBucket globalBucket = new(50, 50, DateTime.UtcNow.AddSeconds(1));
    private readonly ConcurrentDictionary<string, RateLimitBucket> buckets = [];
    private readonly ConcurrentDictionary<string, string> routeHashes = [];

    private readonly ILogger logger;
    private readonly int waitingForHashMilliseconds;

    private bool cancel = false;

    public RateLimitStrategy(ILogger logger, int waitingForHashMilliseconds = 200)
    {
        this.logger = logger;
        this.waitingForHashMilliseconds = waitingForHashMilliseconds;

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
        Ulid traceId = default;
        if (context.Properties.TryGetValue(new("trace-id"), out Ulid? tid) && tid.HasValue)
        {
            traceId = tid.Value;
        }
        else
        {
            traceId = Ulid.Empty;
        }
        

        // get global limit
        bool exemptFromGlobalLimit = false;

        if (context.Properties.TryGetValue(new("exempt-from-global-limit"), out bool exempt))
        {
            exemptFromGlobalLimit = exempt;
        }

        // check against ratelimits now
        DateTime instant = DateTime.UtcNow;

        if (!exemptFromGlobalLimit && !this.globalBucket.CheckNextRequest())
        {
            return this.SynthesizeInternalResponse(route, globalBucket.Reset, "global", traceId);
        }

        if (!this.routeHashes.TryGetValue(route, out string? hash))
        {
            logger.LogTrace
            (
                LoggerEvents.RatelimitDiag,
                "Request ID:{TraceId}: Route has no known hash: {Route}.",
                traceId,
                route
            );

            this.routeHashes.AddOrUpdate(route, "pending", (_, _) => "pending");

            Outcome<HttpResponseMessage> outcome = await action(context, state);

            if (outcome.Result is null)
            {
                this.routeHashes.Remove(route, out _);
                return outcome;
            }

            this.UpdateRateLimitBuckets(outcome.Result, "pending", route);

            // something went awry, just reset and try again next time. this may be because the endpoint didn't return valid headers,
            // which is the case for some endpoints, and we don't need to get hung up on this
            if (this.routeHashes[route] == "pending")
            {
                this.routeHashes.Remove(route, out _);
            }

            return outcome;
        }
        else if (hash == "pending")
        {
            return this.SynthesizeInternalResponse
            (
                route,
                instant + TimeSpan.FromMilliseconds(waitingForHashMilliseconds),
                "route",
                traceId
            );
        }
        else
        {
            RateLimitBucket bucket = this.buckets.GetOrAdd(hash, _ => new());

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
                return this.SynthesizeInternalResponse(route, bucket.Reset, "bucket", traceId);
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
                    return outcome;
                }
            }
            catch (Exception e)
            {
                bucket.CancelReservation();
                return Outcome.FromException<HttpResponseMessage>(e);
            }

            if (!exemptFromGlobalLimit)
            {
                this.UpdateRateLimitBuckets(outcome.Result, hash, route);
            }

            return outcome;
        }
    }

    private Outcome<HttpResponseMessage> SynthesizeInternalResponse(string route, DateTime retry, string scope, Ulid traceId)
    {
        string waitingForRoute = scope == "route" ? " for route hash" : "";

        string traceIdString = "";
        if(this.logger.IsEnabled(LogLevel.Trace))
        {
            traceIdString = $"Request ID:{traceId}: ";
        }
        
        logger.LogDebug
        (
            LoggerEvents.RatelimitPreemptive,
            "{TraceId}Pre-emptive ratelimit for {Route} triggered - waiting{WaitingForRoute} until {Reset:O}.",
            traceIdString,
            route,
            waitingForRoute,
            retry
        );

        return Outcome.FromException<HttpResponseMessage>(
            new PreemptiveRatelimitException(scope, retry - DateTime.UtcNow));
    }

    private void UpdateRateLimitBuckets(HttpResponseMessage response, string oldHash, string route)
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
                this.buckets.AddOrUpdate(newHash, _ => extracted.ToFullBucket(), (_, _) => extracted.ToFullBucket());
            }
            else
            {
                if (this.buckets.TryGetValue(newHash, out RateLimitBucket? oldBucket))
                {
                    oldBucket.UpdateBucket(extracted.Maximum, extracted.Remaining, extracted.Reset);
                }
                else
                {
                    this.buckets.AddOrUpdate(newHash, _ => extracted.ToFullBucket(),
                        (_, _) => extracted.ToFullBucket());
                }
            }

            this.routeHashes.AddOrUpdate(route, newHash!, (_, _) => newHash!);
        }
    }

    private async ValueTask CleanAsync()
    {
        PeriodicTimer timer = new(TimeSpan.FromSeconds(10));

        while (await timer.WaitForNextTickAsync())
        {
            foreach (KeyValuePair<string, string> pair in this.routeHashes)
            {
                if (this.buckets[pair.Value].Reset < DateTime.UtcNow + TimeSpan.FromSeconds(1))
                {
                    this.buckets.Remove(pair.Value, out _);
                    this.routeHashes.Remove(pair.Key, out _);
                }
            }

            if (this.cancel)
            {
                return;
            }
        }
    }

    public void Dispose() => this.cancel = true;
}
