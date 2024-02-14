using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Polly;

namespace DSharpPlus.Net;

using System.Collections.Generic;
using System.Runtime.CompilerServices;

internal class RateLimitStrategy(ILogger logger, int waitingForHashMilliseconds = 200) : ResilienceStrategy<HttpResponseMessage>
{
    private readonly RateLimitBucket globalBucket = new(50, 50, DateTime.UtcNow.AddSeconds(1));
    private readonly ConditionalWeakTable<string, RateLimitBucket> buckets = [];
    private readonly ConcurrentDictionary<string, string> routeHashes = new();
    
    /// <summary>
    /// Collection of routes we are waiting for a hash. This is the case when we have a request for which we dont know the
    /// hash but are waiting for a response to get it.
    /// </summary>
    private readonly List<string> waitingForHashRoutes = [];
    private readonly SemaphoreSlim waitingForHashListSemaphore = new(1, 1);
    
    private static readonly TimeSpan second = TimeSpan.FromSeconds(1);

    protected override async ValueTask<Outcome<HttpResponseMessage>> ExecuteCore<TState>
    (
        Func<ResilienceContext, TState, ValueTask<Outcome<HttpResponseMessage>>> action,
        ResilienceContext context,
        TState state
    )
    {
        // fail-fast if we dont have a route to ratelimit to
        if (!context.Properties.TryGetValue(new("route"), out string? route))
        {
            throw new InvalidOperationException("No route passed. This should be reported to library developers.");
        }

        // get global limit
        bool exemptFromGlobalLimit = false;

        if (context.Properties.TryGetValue(new("exempt-from-global-limit"), out bool exempt))
        {
            exemptFromGlobalLimit = exempt;
        }

        // check against ratelimits now
        DateTime instant = DateTime.UtcNow;

        if (!exemptFromGlobalLimit)
        {
            if (this.globalBucket.Reset < instant)
            {
                this.globalBucket.ResetLimit(instant + second);
            }

            if (!this.globalBucket.CheckNextRequest())
            {
                HttpResponseMessage synthesizedResponse = new(HttpStatusCode.TooManyRequests);

                synthesizedResponse.Headers.RetryAfter = new RetryConditionHeaderValue(this.globalBucket.Reset - instant);
                synthesizedResponse.Headers.Add("DSharpPlus-Internal-Response", "global");

                logger.LogWarning
                (
                    LoggerEvents.RatelimitPreemptive,
                    "Pre-emptive ratelimit triggered - waiting until {reset:yyyy-MM-dd HH:mm:ss zzz}.",
                    this.globalBucket.Reset
                );

                return Outcome.FromResult(synthesizedResponse);
            }
        }

        bool hashPresent = this.routeHashes.TryGetValue(route!, out string? hash);

        if (hash is null)
        {
            logger.LogTrace
            (
                LoggerEvents.RatelimitPreemptive,
                "Route has no known hash: {Route}.",
                route
            );

            //We dont know the hash of the route, so we check if we have a request where we will get the hash. Otherwise
            //we will add the route to our list of routes we are waiting for a hash.
            await this.waitingForHashListSemaphore.WaitAsync(context.CancellationToken);
            if (this.waitingForHashRoutes.Contains(route))
            {
                HttpResponseMessage synthesizedResponse = new(HttpStatusCode.TooManyRequests);

                DateTimeOffset reset = instant.AddMilliseconds(waitingForHashMilliseconds);

                synthesizedResponse.Headers.RetryAfter = new RetryConditionHeaderValue(reset);
                synthesizedResponse.Headers.Add("DSharpPlus-Internal-Response", "waitingOnHash");

                logger.LogWarning
                (
                    LoggerEvents.RatelimitPreemptive,
                    "Pre-emptive ratelimit triggered, waiting for route hash until {reset:yyyy-MM-dd HH:mm:ss zzz}.",
                    reset
                );
                waitingForHashRoutes.Add(route);
                return Outcome.FromResult(synthesizedResponse);
            }

            this.waitingForHashListSemaphore.Release();
        }

        RateLimitBucket? bucket = hash is not null ? this.buckets.GetOrCreateValue(hash) : null;

        if (bucket is not null)
        {
            if (!bucket.CheckNextRequest())
            {
                HttpResponseMessage synthesizedResponse = new(HttpStatusCode.TooManyRequests);

                synthesizedResponse.Headers.RetryAfter = new RetryConditionHeaderValue(bucket.Reset - instant);
                synthesizedResponse.Headers.Add("DSharpPlus-Internal-Response", "bucket");

                logger.LogWarning
                (
                    LoggerEvents.RatelimitPreemptive,
                    "Pre-emptive ratelimit triggered - waiting until {reset:yyyy-MM-dd HH:mm:ss zzz}.",
                    bucket.Reset
                );

                return Outcome.FromResult(synthesizedResponse);
            }
        }
        else
        {
            logger.LogTrace
            (
                LoggerEvents.RatelimitDiag,
                "Route has no known bucket: {Route}.",
                route
            );
        }

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
            bucket?.CancelReservation();
            throw;
        }

        HttpResponseMessage response = outcome.Result;

        bool hasBucketHeader = response.Headers.TryGetValues("X-RateLimit-Bucket", out IEnumerable<string>? hashHeader);

        if (!exemptFromGlobalLimit && hasBucketHeader)
        {
            hash = hashHeader?.Single();

            if(!RateLimitBucket.TryExtractRateLimitBucket(response.Headers, out RateLimitBucket? extracted))
            {
                return outcome;
            }
            else if (!hashPresent)
            {
                this.buckets.AddOrUpdate(hash!, extracted);
            }
            else
            {
                bucket!.UpdateBucket(extracted.remaining);
            }

            // the request had no known hash, we remove the route from the waiting list because we got a hash now
            if (!hashPresent)
            {
                await this.waitingForHashListSemaphore.WaitAsync(context.CancellationToken);
                this.waitingForHashRoutes.Remove(route!);
                this.waitingForHashListSemaphore.Release();
            }

            this.routeHashes.AddOrUpdate(route!, hash!, (_, _) => hash!);
        }

        return outcome;
    }
}
