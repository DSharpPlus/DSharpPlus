// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

using Polly;

namespace DSharpPlus.Core.Rest.Ratelimiting
{
    // main ratelimit handler
    internal class PollyRatelimitingPolicy : AsyncPolicy<HttpResponseMessage>
    {
        // keeps track of endpoint -> bucket hash mapping
        private readonly ConcurrentDictionary<string, string> endpointBuckets;

        // global ratelimit bucket of 50 requests per second
        private readonly RatelimitBucket globalBucket;

        // one second, we use this a lot so don't reallocate every time
        private static readonly TimeSpan oneSecond = TimeSpan.FromSeconds(1);

        public PollyRatelimitingPolicy()
        {
            globalBucket = new(50, 50, DateTimeOffset.UtcNow + oneSecond, "global");
            endpointBuckets = new();
        }

        // the main policy.
        protected override async Task<HttpResponseMessage> ImplementationAsync(
            Func<Context, CancellationToken, Task<HttpResponseMessage>> action,
            Context context,
            CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            // ensure a valid endpoint was passed
            if (!context.TryGetValue("endpoint", out object endpointRaw) || endpointRaw is not string endpoint)
            {
                throw new InvalidOperationException("No endpoint passed.");
            }

            // ensure a valid cache was passed. the cache mostly serves as an alternative to v4's "BucketCleanerTask",
            // which, in its static manner, would hardly work with the very DI-focused architecture of Polly and Polly policies.
            if (!context.TryGetValue("cache", out object cacheRaw) || cacheRaw is not IMemoryCache cache)
            {
                throw new InvalidOperationException("No cache passed.");
            }

            // avoid having to create multiple of these. execution time should be reasonably fast.
            DateTimeOffset instant = DateTimeOffset.UtcNow;

            // some endpoints are exempt from the global limit, account for them.
            bool subjectToGlobalLimit = true;

            // this is our unique identifier for each active bucket.
            string bucketHash;

            // make sure to reflect context-passed exemption
            if (context.TryGetValue("subject-to-global-limit", out object globallyLimitedRaw) && globallyLimitedRaw is bool globallyLimited)
            {
                subjectToGlobalLimit = globallyLimited;
            }

            // the following logic applies only to globally ratelimited requests.
            if (subjectToGlobalLimit)
            {
                // if the bucket is already expired, reset it. technically we could save a bit of time by goto-ing out of the
                // if clause entirely if this check succeeds, since we are then guaranteed the request will be allowed.
                if (globalBucket.ResetTime < instant)
                {
                    globalBucket.ResetBucket(instant + oneSecond);
                }

                if (!globalBucket.AllowNextRequest())
                {
                    HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);

                    response.Headers.RetryAfter = new(globalBucket.ResetTime - instant);

                    // allow users to figure out whether the 429 showing up in their debug windows was thrown by Discord or
                    // an internal DSharpPlus response. also specify what bucket denied the request.
                    response.Headers.Add("X-DSharpPlus-Preemptive-Ratelimit", "global");

                    // pretend this was a discord api call and return the response.
                    return response;
                }
            }

            // if we don't already have this bucket cached, set the hash to our endpoint for now. 
            // this will later be used to identify a newly called bucket.
            bucketHash = endpointBuckets.TryGetValue(endpoint, out string? nullableBucketHash)
                ? nullableBucketHash
                : endpoint;

            // get the cached ratelimit bucket. if it doesn't exist, we are guaranteed our request will be allowed
            // as it will be the first request to this bucket, which never 429s.
            if (cache.TryGetValue(bucketHash, out RatelimitBucket? cachedBucket) || !cachedBucket!.AllowNextRequest())
            {
                HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);

                response.Headers.RetryAfter = new(cachedBucket!.ResetTime - instant);

                // allow users to figure out whether the 429 showing up in their debug windows was thrown by Discord or
                // an internal DSharpPlus response. also specify what bucket denied the request.
                response.Headers.Add("X-DSharpPlus-Preemptive-Ratelimit", "endpoint");

                // pretend this was a discord api call and return the response.
                return response;
            }

            // actually make our api call now. we can remove the ConfigureAwait call or always pass false if we want.
            HttpResponseMessage discordResponse = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            // try extracting a new ratelimit bucket from this, even if we already had one cached.
            // discord can decide to change the limit on us, in which case we need a new bucket with a new hash.
            if(!RatelimitBucket.TryExtractRatelimitBucket(discordResponse.Headers, out RatelimitBucket? extractedBucket))
            {
                return discordResponse;
            }

            if(extractedBucket.Hash == null)
            {
                // make sure we dont end up with chaotic state first
                endpointBuckets.TryRemove(endpoint, out _);
                cache.Set(endpoint, extractedBucket, extractedBucket.ResetTime + oneSecond);

                return discordResponse;
            }

            // wow, everything went well. update the ratelimit bucket and return

            endpointBuckets.AddOrUpdate(endpoint, extractedBucket.Hash, (_, _) => extractedBucket.Hash);

            // expire one second after the bucket expires, to avoid race conditions.
            cache.Set(endpoint, extractedBucket, extractedBucket.ResetTime + oneSecond);

            // remove potential stale data
            if(extractedBucket.Hash != bucketHash)
            {
                cache.Remove(bucketHash);
            }

            return discordResponse;
        }
    }
}
