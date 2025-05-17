using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Polly;

namespace DSharpPlus.Net;

internal class RateLimitStrategy : ResilienceStrategy<HttpResponseMessage>, IDisposable
{
    private readonly RateLimitBucket globalBucket;
    private readonly ConcurrentDictionary<string, RateLimitBucket> buckets = [];
    private readonly ConcurrentDictionary<string, string> routeHashes = [];

    private readonly ILogger logger;
    private readonly int waitingForHashMilliseconds;

    private readonly Lock bucketCheckingLock = new();

    private bool cancel = false;

    public RateLimitStrategy(ILogger logger, int waitingForHashMilliseconds = 200, int maximumRestRequestsPerSecond = 15)
    {
        this.logger = logger;
        this.waitingForHashMilliseconds = waitingForHashMilliseconds;
        this.globalBucket = new(maximumRestRequestsPerSecond, maximumRestRequestsPerSecond, DateTime.UtcNow.AddSeconds(1));
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

        // if we're exempt, execute immediately
        if (context.Properties.TryGetValue(new("exempt-from-all-limits"), out bool allExempt) && allExempt)
        {
            this.logger.LogTrace
            (
                LoggerEvents.RatelimitDiag,
                "Request ID:{TraceId}: Executing request exempt from all ratelimits to {Route}",
                traceId,
                route
            );

            return await action(context, state);
        }

        // get global limit
        bool exemptFromGlobalLimit = false;

        if (context.Properties.TryGetValue(new("exempt-from-global-limit"), out bool exempt))
        {
            exemptFromGlobalLimit = exempt;
        }

        // check against ratelimits now
        DateTime instant = DateTime.UtcNow;

        lock (this.bucketCheckingLock)
        {
            if (!exemptFromGlobalLimit && !this.globalBucket.CheckNextRequest())
            {
                return SynthesizeInternalResponse(route, this.globalBucket.Reset, "global", traceId);
            }
        }

        if (!this.routeHashes.TryGetValue(route, out string? hash))
        {
            if (!this.routeHashes.TryAdd(route, "pending"))
            {
                // two different async requests entered this at the same time, requeue this one
                return SynthesizeInternalResponse
                (
                    route,
                    instant + TimeSpan.FromMilliseconds(this.waitingForHashMilliseconds),
                    "route",
                    traceId
                );
            }

            this.logger.LogTrace
            (
                LoggerEvents.RatelimitDiag,
                "Request ID:{TraceId}: Route has no known hash: {Route}.",
                traceId,
                route
            );

            Outcome<HttpResponseMessage> outcome = await action(context, state);

            if (!exemptFromGlobalLimit)
            {
                this.globalBucket.CompleteReservation();
            }

            if (outcome.Result is null)
            {
                this.routeHashes.Remove(route, out _);
                return outcome;
            }

            UpdateRateLimitBuckets(outcome.Result, "pending", route, traceId);

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
            if (!exemptFromGlobalLimit)
            {
                this.globalBucket.CancelReservation();
            }

            return SynthesizeInternalResponse
            (
                route,
                instant + TimeSpan.FromMilliseconds(this.waitingForHashMilliseconds),
                "route",
                traceId
            );
        }
        else
        {
            RateLimitBucket bucket = this.buckets.GetOrAdd(hash, _ => new());

            this.logger.LogTrace
            (
                LoggerEvents.RatelimitDiag,
                "Request ID:{TraceId}: Checking bucket, current state is [Remaining: {Remaining}, Reserved: {Reserved}]",
                traceId,
                bucket.remaining,
                bucket.reserved
            );

            lock (this.bucketCheckingLock)
            {
                if (!bucket.CheckNextRequest())
                {
                    if (!exemptFromGlobalLimit)
                    {
                        this.globalBucket.CancelReservation();
                    }

                    return SynthesizeInternalResponse(route, bucket.Reset, "bucket", traceId);
                }
            }

            this.logger.LogTrace
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
                        this.globalBucket.CancelReservation();
                    }

                    return outcome;
                }

                if (!exemptFromGlobalLimit)
                {
                    this.globalBucket.CompleteReservation();
                }
            }
            catch (Exception e)
            {
                if (!exemptFromGlobalLimit)
                {
                    this.globalBucket.CancelReservation();
                }

                bucket.CancelReservation();
                return Outcome.FromException<HttpResponseMessage>(e);
            }

            if (!exemptFromGlobalLimit)
            {
                UpdateRateLimitBuckets(outcome.Result, hash, route, traceId);
            }

            if (outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests)
            {
                string resetAfterRaw = outcome.Result.Headers.GetValues("X-RateLimit-Reset-After").Single();
                TimeSpan resetAfter = TimeSpan.FromSeconds(double.Parse(resetAfterRaw));

                string traceIdString = "";
                if (this.logger.IsEnabled(LogLevel.Trace))
                {
                    traceIdString = $"Request ID:{traceId}: ";
                }

                this.logger.LogWarning
                (
                    "{TraceId}Hit Discord ratelimit on route {Route}, waiting for {ResetAfter}",
                    traceIdString,
                    route,
                    resetAfter
                );

                return Outcome.FromException<HttpResponseMessage>(new RetryableRatelimitException(resetAfter));
            }

            return outcome;
        }
    }

    private Outcome<HttpResponseMessage> SynthesizeInternalResponse(string route, DateTime retry, string scope, Ulid traceId)
    {
        string waitingForRoute = scope == "route" ? " for route hash" : "";
        string global = scope == "global" ? " global" : "";

        string traceIdString = "";
        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            traceIdString = $"Request ID:{traceId}: ";
        }

        DateTime retryJittered = retry + TimeSpan.FromMilliseconds(Random.Shared.NextInt64(100));

        this.logger.LogDebug
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
                this.logger.LogTrace("Request ID:{ID} - Initial bucket capacity: {max}", id, extracted.Maximum);
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
                    this.logger.LogTrace("Request ID:{ID} - Initial bucket capacity: {max}", id, extracted.Maximum);
                    this.buckets.AddOrUpdate(newHash, _ => extracted.ToFullBucket(),
                        (_, _) => extracted.ToFullBucket());
                }
            }

            this.routeHashes.AddOrUpdate(route, newHash!, (_, _) => newHash!);
        }
    }

    private async Task CleanAsync()
    {
        PeriodicTimer timer = new(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync())
        {
            foreach (KeyValuePair<string, string> pair in this.routeHashes)
            {
                if (this.buckets.TryGetValue(pair.Value, out RateLimitBucket? bucket) && bucket.Reset < DateTime.UtcNow + TimeSpan.FromSeconds(1))
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
