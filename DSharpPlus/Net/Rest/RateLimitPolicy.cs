using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Polly;

namespace DSharpPlus.Net;

using System.Collections.Generic;

internal class RateLimitPolicy : AsyncPolicy<HttpResponseMessage>
{
    private readonly RateLimitBucket globalBucket;
    private readonly MemoryCache cache;
    private readonly ILogger logger;
    private readonly ConcurrentDictionary<string, string> routeHashes;
    
    /// <summary>
    /// Collection of routes we are waiting for a hash. This is the case when we have a request for which we dont know the
    /// hash but are waiting for a response to get it.
    /// </summary>
    private List<string> waitingForHashRoutes;
    private SemaphoreSlim waitingForHashListSemaphore = new(1, 1);
    
    /// <summary>
    /// Time to wait for a response to set the hash for a route.
    /// </summary>
    private readonly int waitingForHashMilliseconds = 200;
    private static readonly TimeSpan second = TimeSpan.FromSeconds(1);

    public RateLimitPolicy(ILogger logger)
    {
        this.globalBucket = new(50, 50, DateTime.UtcNow.AddSeconds(1));

        // we don't actually care about any settings on this cache
        // in a future day and age, we'll hopefully replace this with an application-global cache, but oh well
        this.cache = new
        (
            new MemoryCacheOptions
            {
            }
        );

        this.logger = logger;
        this.routeHashes = new();
    }

    protected override async Task<HttpResponseMessage> ImplementationAsync
    (
        Func<Context, CancellationToken, Task<HttpResponseMessage>> action,
        Context context,
        CancellationToken cancellationToken,
        // since the library doesn't use CA(false) at all (#1533), we will proceed to ignore this 
        bool continueOnCapturedContext = true
    )
    {
        // fail-fast if we dont have a route to ratelimit to
        if (!context.TryGetValue("route", out object rawRoute) || rawRoute is not string route)
        {
            throw new InvalidOperationException("No route passed. This should be reported to library developers.");
        }

        // get global limit
        bool exemptFromGlobalLimit = false;

        if (context.TryGetValue("exempt-from-global-limit", out object rawExempt) && rawExempt is bool exempt)
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

                this.logger.LogWarning
                (
                    LoggerEvents.RatelimitPreemptive,
                    "Pre-emptive ratelimit triggered - waiting until {reset:yyyy-MM-dd HH:mm:ss zzz}.",
                    this.globalBucket.Reset
                );

                return synthesizedResponse;
            }
        }

        bool hashPresent = this.routeHashes.TryGetValue(route, out string? hash);

        if (hash is not null)
        {
            RateLimitBucket? bucket = this.cache.Get<RateLimitBucket?>(hash);

            if (bucket is not null)
            {
                if (!bucket.Value.CheckNextRequest())
                {
                    HttpResponseMessage synthesizedResponse = new(HttpStatusCode.TooManyRequests);

                    synthesizedResponse.Headers.RetryAfter = new RetryConditionHeaderValue(bucket.Value.Reset - instant);
                    synthesizedResponse.Headers.Add("DSharpPlus-Internal-Response", "bucket");

                    this.logger.LogWarning
                    (
                        LoggerEvents.RatelimitPreemptive,
                        "Pre-emptive ratelimit triggered - waiting until {reset:yyyy-MM-dd HH:mm:ss zzz}.",
                        bucket.Value.Reset
                    );

                    return synthesizedResponse;
                }
            }
            else
            {
                this.logger.LogTrace
                (
                    LoggerEvents.RatelimitDiag,
                    "Route has no known bucket: {Route}.",
                    route
                );
            }
        }
        else
        {
            this.logger.LogTrace
            (
                LoggerEvents.RatelimitPreemptive,
                "Route has no known hash: {Route}.",
                route
            );
            
            //We dont know the hash of the route, so we check if we have a request where we will get the hash. Otherwise
            //we will add the route to our list of routes we are waiting for a hash.
            await this.waitingForHashListSemaphore.WaitAsync(cancellationToken);
            if (this.waitingForHashRoutes.Contains(route))
            {
                HttpResponseMessage synthesizedResponse = new(HttpStatusCode.TooManyRequests);
                
                DateTimeOffset reset = instant.AddMilliseconds(this.waitingForHashMilliseconds);

                synthesizedResponse.Headers.RetryAfter = new RetryConditionHeaderValue(reset);
                synthesizedResponse.Headers.Add("DSharpPlus-Internal-Response", "waitingOnHash");

                this.logger.LogWarning
                (
                    LoggerEvents.RatelimitPreemptive,
                    "Pre-emptive ratelimit triggered, waiting for route hash until {reset:yyyy-MM-dd HH:mm:ss zzz}.",
                    reset
                );
                waitingForHashRoutes.Add(route);
                return synthesizedResponse;
            }
            
            this.waitingForHashListSemaphore.Release();
        }

        // make the actual request

        HttpResponseMessage response = await action(context, cancellationToken);
        bool hasBucketHeader = response.Headers.TryGetValues("X-RateLimit-Bucket", out IEnumerable<string>? hashHeader);

        if (!exemptFromGlobalLimit && hasBucketHeader)
        {
            // the request had no known hash, we remove the route from the waiting list because we got a hash now
            if (!hashPresent)
            {
                await this.waitingForHashListSemaphore.WaitAsync(cancellationToken);
                this.waitingForHashRoutes.Remove(route);
                this.waitingForHashListSemaphore.Release();
            }
            
            hash = hashHeader?.Single();

            if(!RateLimitBucket.TryExtractRateLimitBucket(response.Headers, out RateLimitBucket? extracted))
            {
                return response;
            }

            this.cache.CreateEntry(route)
                .SetValue(extracted.Value)
                .Dispose();

            this.routeHashes.AddOrUpdate(route, hash, (_, _) => hash);
        }
        return response;
    }
}
