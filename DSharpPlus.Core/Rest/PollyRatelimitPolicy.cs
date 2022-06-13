using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;

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
            if (!context.TryGetValue("cache", out object cacheObject) || cacheObject is not IDistributedCache cache)
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
            Task<byte[]?> awaitableBucket = cache.GetAsync(endpoint, cancellationToken);

            // apply global ratelimits
            if (subjectToGlobalLimit)
            {
                // obtain the global bucket
                byte[]? serializedGlobalBucket = await cache.GetAsync("global", cancellationToken);

                RatelimitBucket? globalBucket = JsonSerializer.Deserialize(serializedGlobalBucket, BucketSerializationContext.Default.Context);

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

                // serialize and write the global bucket again.
                string serializedGlobal = JsonSerializer.Serialize(globalBucket, BucketSerializationContext.Default.Context);

                _ = cache.SetStringAsync("global", serializedGlobal, cancellationToken);
            }

            // if we don't already have this bucket cached, set the hash to our endpoint for now. 
            // this will later be used to identify a newly called bucket.
            string hash = __endpoint_hash_mapping.TryGetValue(endpoint, out string? nullableBucketHash)
                ? nullableBucketHash
                : endpoint;

            // get the cached bucket and see whether it lets us request.
            // importantly, we use the context-passed endpoints to identify buckets here.
            byte[]? serializedBucket = await awaitableBucket;

            // keep a dummy bucket here. we'll either create a new one or deserialize one.
            RatelimitBucket? bucket = null;

            if (serializedBucket is not null)
            {
                bucket = JsonSerializer.Deserialize(serializedBucket.AsSpan(), BucketSerializationContext.Default.Context);

                // if our bucket is null for some reason, consider the request denied to avoid accidental 429s.
                if (!bucket?.AllowNextRequest() ?? false)
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
            string serialized = JsonSerializer.Serialize(bucket!, BucketSerializationContext.Default.Context);
            _ = cache.SetStringAsync(endpoint, serialized, cancellationToken);

            // store the new hash, if we have a new one
            __endpoint_hash_mapping[endpoint] = bucket!.Hash;

            // remove potential stale data
            if (hash != bucket!.Hash)
            {
                _ = cache.RemoveAsync(hash, cancellationToken);
            }

            return discordResponse;
        }
    }
}
