using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Caching.Abstractions;

using Polly;

namespace DSharpPlus.Core.Rest
{
    // main ratelimit handler
    internal class PollyRatelimitPolicy : AsyncPolicy<HttpResponseMessage>
    {
        // keeps track of endpoint -> bucket hash mapping
        private readonly ConcurrentDictionary<string, string> __endpoint_hash_mapping;

        // stores one second, a very common thing, so we don't want to allocate it every time
        private static readonly TimeSpan __one_second = TimeSpan.FromSeconds(1);

        private const string GlobalBucketName = "DSharpPlus:Ratelimiting:global";

        public PollyRatelimitPolicy()
            => __endpoint_hash_mapping = new();


        protected override async Task<HttpResponseMessage> ImplementationAsync
        (
            Func<Context, CancellationToken, Task<HttpResponseMessage>> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext = true
        )
        {
            // ascertain a valid endpoint
            if (!context.TryGetValue("endpoint", out object endpointObject) || endpointObject is not string endpoint)
            {
                throw new InvalidOperationException("No valid endpoint provided.");
            }

            //ascertain a valid cache.
            if (!context.TryGetValue("cache", out object cacheObject) || cacheObject is not ICacheService cache)
            {
                throw new InvalidOperationException("No valid cache provided.");
            }

            // some endpoints are exempt from the global limit. account for them.
            bool subjectToGlobalLimit = true;

            if (context.TryGetValue("subject-to-global-limit", out object globalLimitObject)
                && globalLimitObject is bool globallyLimited)
            {
                subjectToGlobalLimit = globallyLimited;
            }

            // already make our request, await it later
            ValueTask<bool> awaitableBucket = cache.TryGetAsync(endpoint, out RatelimitBucket bucket);

            // apply global ratelimits
            // todo: handle a missing global bucket
            if (subjectToGlobalLimit && await cache.TryGetAsync(GlobalBucketName, out RatelimitBucket globalBucket))
            {
                if (globalBucket is null)
                {
                    throw new InvalidOperationException("No global ratelimit bucket could be found.");
                }

                // store the current instant for a second here
                DateTimeOffset instant = DateTimeOffset.UtcNow;

                // if the bucket is already expired, reset it. technically we could save a bit of time by goto-ing out of the
                // if clause entirely if this check succeeds, since we are then guaranteed the request will be allowed by the global bucket.
                if (globalBucket.ResetTime < instant)
                {
                    globalBucket.ResetBucket(instant + __one_second);
                }

                if (!globalBucket.AllowNextRequest())
                {
                    HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);

                    response.Headers.RetryAfter = new(globalBucket.ResetTime - instant);

                    // allow users to figure out whether the 429 showing up in their debug windows was thrown by Discord or DSharpPlus
                    response.Headers.Add("X-DSharpPlus-Preemptive-RateLimit", "global");

                    // pretend this was a discord api call and return the response.
                    // the PollyRetryPolicy will see this 429 and retry the request later.
                    return response;
                }

                await cache.CacheAsync(GlobalBucketName, globalBucket);
            }

            // if we don't already have this bucket cached, set the hash to our endpoint for now. 
            // this will later be used to identify a newly called bucket.
            string hash = __endpoint_hash_mapping.TryGetValue(endpoint, out string? nullableBucketHash)
                ? nullableBucketHash
                : endpoint;

            if (await awaitableBucket)
            {
                // if our bucket is null for some reason, consider the request denied to avoid accidental 429s.
                if (bucket?.AllowNextRequest() ?? false)
                {
                    HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);

                    // i'd reuse our instant from earlier but we have since potentially taken a lot of time to obtain the bucket.
                    response.Headers.RetryAfter = new(bucket!.ResetTime - DateTimeOffset.UtcNow);

                    // allow users to figure out whether the 429 showing up in their debug windows was thrown by Discord or DSharpPlus
                    response.Headers.Add("X-DSharpPlus-Preemptive-RateLimit", "global");

                    // pretend this was a discord api call and return the response.
                    // the PollyRetryPolicy will see this 429 and retry the request later.
                    return response;
                }
            }

            // finally, make our API call.
            HttpResponseMessage discordResponse = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            // ensure the bucket is set.
            // also, if we got a new hash, obtain a completely new bucket.
            if (bucket is not null && hash == discordResponse.Headers.GetValues("X-RateLimit-Name").SingleOrDefault())
            {
                _ = bucket.TryUpdateCurrentBucket(discordResponse.Headers);
            }
            else
            {
                RatelimitBucket.ExtractRatelimitBucket(discordResponse.Headers, out bucket);
            }

            // cache the new bucket
            await cache.CacheAsync(endpoint, bucket);

            // store the new hash, if we have a new one
            __endpoint_hash_mapping[endpoint] = bucket!.Hash;

            // remove potential stale data
            if (hash != bucket!.Hash)
            {
                await cache.RemoveAsync(endpoint);
            }

            return discordResponse;
        }
    }
}
