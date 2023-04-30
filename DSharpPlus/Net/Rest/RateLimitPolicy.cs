using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Polly;

namespace DSharpPlus.Net;

internal class RateLimitPolicy : AsyncPolicy<HttpResponseMessage>
{
    private readonly RateLimitBucket globalBucket;
    private readonly MemoryCache cache;
    private readonly ILogger logger;
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
        if(!context.TryGetValue("route", out object rawRoute) || rawRoute is not string route)
        {
            throw new InvalidOperationException("No route passed. This should be reported to library developers.");
        }

        // get global limit
        bool exemptFromGlobalLimit = false;

        if(context.TryGetValue("exempt-from-global-limit", out object rawExempt) && rawExempt is bool exempt)
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

        RateLimitBucket? bucket = this.cache.Get<RateLimitBucket?>(route);

        if(bucket is not null)
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

        // make the actual request

        HttpResponseMessage response = await action(context, cancellationToken);

        if(!RateLimitBucket.TryExtractRateLimitBucket(response.Headers, out RateLimitBucket? extracted))
        {
            return response;
        }

        this.cache.CreateEntry(route)
                .SetValue(extracted.Value)
                .Dispose();

        return response;
    }
}
