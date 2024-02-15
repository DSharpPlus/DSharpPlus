using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Polly;

namespace DSharpPlus.Net;

internal class RateLimitStrategy : ResilienceStrategy<HttpResponseMessage>
{
    private readonly RateLimitBucket globalBucket = new(50, 50, DateTime.UtcNow.AddSeconds(1));
    private readonly ConcurrentDictionary<string, RateLimitBucket> buckets = [];
    private readonly ConcurrentDictionary<string, string> routeHashes = [];

    private readonly ValueTask ratelimitCleanerTask;

    private readonly ILogger logger;
    private readonly int waitingForHashMilliseconds;

    public RateLimitStrategy(ILogger logger, int waitingForHashMilliseconds = 200)
    {
        this.logger = logger;
        this.waitingForHashMilliseconds = waitingForHashMilliseconds;

        this.ratelimitCleanerTask = CleanAsync();
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
            throw new InvalidOperationException("No route passed. This should be reported to library developers.");
        }
#pragma warning restore CS8600

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
            return this.SynthesizeInternalResponse(route, globalBucket.Reset, "global");
        }

        if (!this.routeHashes.TryGetValue(route, out string? hash))
        {
            logger.LogTrace
            (
                LoggerEvents.RatelimitDiag,
                "Route has no known hash: {Route}.",
                route
            );

            this.routeHashes.AddOrUpdate(route, "pending", (_, _) => "pending");

            Outcome<HttpResponseMessage> outcome = await action(context, state);

            if (outcome.Result is null)
            {
                return outcome;
            }

            if (!exemptFromGlobalLimit)
            {
                this.UpdateRateLimitBuckets(outcome.Result, "pending", route);
            }

            return outcome;
        }
        else if (hash == "pending")
        {
            return this.SynthesizeInternalResponse
            (
                route,
                instant + TimeSpan.FromMilliseconds(waitingForHashMilliseconds),
                "route"
            );
        }
        else
        {
            RateLimitBucket bucket = this.buckets.GetOrAdd(hash, _ => new());

            logger.LogTrace
            (
                LoggerEvents.RatelimitDiag, 
                "Checking request, current state is [Remaining: {Remaining}, Reserved: {Reserved}]", 
                bucket.remaining, 
                bucket.reserved
            );

            if (!bucket.CheckNextRequest())
            {
                return this.SynthesizeInternalResponse(route, bucket.Reset, "bucket");
            }

            logger.LogTrace
            (
                LoggerEvents.RatelimitDiag, 
                "Allowed request, current state is [Remaining: {Remaining}, Reserved: {Reserved}]", 
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
            catch
            {
                bucket.CancelReservation();
                throw;
            }

            if (!exemptFromGlobalLimit)
            {
                this.UpdateRateLimitBuckets(outcome.Result, hash, route);
            }

            return outcome;
        }
    }

    private Outcome<HttpResponseMessage> SynthesizeInternalResponse(string route, DateTime retry, string scope)
    {
        HttpResponseMessage synthesizedResponse = new(HttpStatusCode.TooManyRequests);

        synthesizedResponse.Headers.RetryAfter = new RetryConditionHeaderValue
        (
            retry + TimeSpan.FromMilliseconds(Random.Shared.NextInt64(50))
        );

        synthesizedResponse.Headers.Add("DSharpPlus-Internal-Response", scope);

        string waitingForRoute = scope == "route" ? " for route hash" : "";

        logger.LogDebug
        (
            LoggerEvents.RatelimitPreemptive,
            "Pre-emptive ratelimit for {Route} triggered - waiting{WaitingForRoute} until {Reset:yyyy-MM-dd HH:mm:ss zzz}.",
            route,
            waitingForRoute,
            retry
        );

        throw new PreemptiveRatelimitException(scope, retry - DateTime.UtcNow);
    }

    private void UpdateRateLimitBuckets(HttpResponseMessage response, string oldHash, string route)
    {
        if (response.Headers.TryGetValues("X-RateLimit-Bucket", out IEnumerable<string>? hashHeader))
        {
            string newHash = hashHeader?.Single()!;

            if (!RateLimitBucket.TryExtractRateLimitBucket(response.Headers, out RateLimitBucket? extracted))
            {
                return;
            }
            else if (oldHash != newHash)
            {
                this.buckets.AddOrUpdate(newHash, _ => extracted, (_, _) => extracted);
            }
            else
            {
                if (this.buckets.TryGetValue(newHash, out RateLimitBucket? oldBucket))
                {
                    oldBucket.UpdateBucket(extracted.maximum, extracted.remaining, extracted.Reset);
                }
                else
                {
                    this.buckets.AddOrUpdate(newHash, _ => extracted, (_, _) => extracted);
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
        }
    }
}
