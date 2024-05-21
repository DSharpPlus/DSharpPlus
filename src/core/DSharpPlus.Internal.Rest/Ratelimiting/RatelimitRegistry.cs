// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable CA1812, CA2008

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Rest;
using DSharpPlus.Results;
using DSharpPlus.Results.Errors;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using NonBlocking;

using Polly;

namespace DSharpPlus.Internal.Rest.Ratelimiting;

/// <summary>
/// Contains and manages ratelimits as far as known.
/// </summary>
public sealed class RatelimitRegistry : IRatelimitRegistry
{
    private readonly ConcurrentDictionary<Snowflake, float> webhook429s = new();
    private readonly ConcurrentDictionary<StringSegment, float> route429s = new();
    private readonly ConcurrentDictionary<string, string> hashes = new();
    private readonly ConditionalWeakTable<string, RatelimitBucket> ratelimitBuckets = [];
    private readonly ILogger<IRestClient> logger;

    private readonly double timeReferencePoint;

    private bool dirty;

    public RatelimitRegistry
    (
        ILogger<IRestClient> logger,
        IOptions<RatelimitOptions> options
    )
    {
        this.logger = logger;

        // ExecuteSynchronously ensures we never leave that thread. 
        TaskFactory factory = options.Value.UseSeparateCleanupThread
            ? new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.ExecuteSynchronously)
            : Task.Factory;

        _ = factory.StartNew(() => this.CleanupSimpleRatelimitsAsync(options.Value.CleanupInterval, options.Value.Token));
        this.timeReferencePoint = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    private async ValueTask CleanupSimpleRatelimitsAsync(int interval, CancellationToken ct)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(interval));

        // 2x the interval, rounded down
        int decay = interval / 500;
        int counter = 0;

        startingCleanupLoop(this.logger, interval, decay, null);

        while (await timer.WaitForNextTickAsync(ct))
        {
            if (!this.dirty)
            {
#pragma warning disable CA1848 // there's no performance improvement to be gained from not having any placeholders.
                this.logger.LogTrace("There was no simple ratelimit added since last cleanup, skipping.");
                continue;
#pragma warning restore CA1848
            }

            double currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            initiatingCleanupMessage(this.logger, "webhook", this.webhook429s.Count, null);

            // we iterate on a snapshot. this leads to rare race conditions if a bucket is updated while we're iterating,
            // but since we only remove considerably outdated buckets that have already reset a while ago, the worst that
            // can happen is that we delete the first response, the second will then be correctly limited - unless the bucket
            // is extremely long-lived and we hit this condition on every request. this is unlikely enough to assume it
            // doesn't happen, i believe.
            foreach (KeyValuePair<Snowflake, float> pair in this.webhook429s)
            {
                if (currentTime > pair.Value + this.timeReferencePoint)
                {
                    _ = this.webhook429s.Remove(pair.Key, out _);
                }
            }

            completedCleanupMessage(this.logger, "webhook", this.webhook429s.Count, null);
            initiatingCleanupMessage(this.logger, "route", this.route429s.Count, null);

            foreach (KeyValuePair<StringSegment, float> pair in this.route429s)
            {
                if (currentTime > pair.Value + this.timeReferencePoint)
                {
                    _ = this.route429s.Remove(pair.Key, out _);
                }
            }

            completedCleanupMessage(this.logger, "route", this.route429s.Count, null);

            // only clean buckets every ten iterations. this can be changed later
            if (counter >= 10)
            {
                initiatingHashCleanupMessage(this.logger, this.hashes.Count, null);

                foreach (KeyValuePair<string, string> pair in this.hashes)
                {
                    if (this.ratelimitBuckets.TryGetValue(pair.Key, out RatelimitBucket? bucket))
                    {
                        if (currentTime > bucket.Expiry + this.timeReferencePoint)
                        {
                            // since we use a CWT, the bucket will automatically die once all routes holding on to it are dead
                            _ = this.hashes.Remove(pair.Key, out _);
                        }
                    }
                }

                completedHashCleanupMessage(this.logger, this.hashes.Count, null);
                counter = 0;
            }

            this.dirty = false;
            counter++;
        }
    }

    /// <inheritdoc/>
    public Result<bool> CheckRatelimit(HttpRequestMessage request)
    {
        Context? context = request.GetPolicyExecutionContext();
        double currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        string route = context is not null && context.TryGetValue("route", out object rawRoute) && rawRoute is string contextRoute
            ? contextRoute
            : $"{request.Method.ToString().ToUpperInvariant()} {request.RequestUri}";

        // we know the route - easy enough
        if (this.hashes.TryGetValue(route, out string? hash) && this.ratelimitBuckets.TryGetValue(hash, out RatelimitBucket? bucket))
        {
            bool success = bucket.Remaining - bucket.Reserved > 0;

            if (!success)
            {
                rejectedRatelimit(this.logger, request.RequestUri!.AbsoluteUri, "conclusively", null);
            }

            bucket.Reserve();
            return success;
        }

        // what if we don't know this route? check whether we know related routes, answer heuristically based on that
        int index = route.IndexOf(' ', StringComparison.Ordinal);
        StringSegment segment = new(route, index + 1, route.Length - index - 1);

        if (this.route429s.TryGetValue(segment, out float last429Expiry))
        {
            bool success = currentTime > this.timeReferencePoint + last429Expiry;

            if (!success)
            {
                rejectedRatelimit(this.logger, request.RequestUri!.AbsoluteUri, "speculatively based on route ratelimits", null);
            }

            return success;
        }

        // we don't know related routes yet. if it's a webhook route, we check the simple route. for other kinds of routes we
        // can impl heuristics here too, but not yet
        if
        (
            context is not null
            && context.TryGetValue("simple-route", out object rawSimpleRoute)
            && rawSimpleRoute is SimpleRatelimitRoute simpleRoute
            && simpleRoute.Resource == TopLevelResource.Webhook
        )
        {
            if (this.webhook429s.TryGetValue(simpleRoute.Id, out last429Expiry))
            {
                bool success = currentTime > this.timeReferencePoint + last429Expiry;

                if (!success)
                {
                    rejectedRatelimit(this.logger, request.RequestUri!.AbsoluteUri, "speculatively based on webhook ratelimits", null);
                }

                return success;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public Result UpdateRatelimit(HttpRequestMessage request, HttpResponseMessage response)
    {
        // extract the reset time, immediately bail if we couldn't find one
        HttpResponseHeaders headers = response.Headers;
        double reset = 0;

        if
        (
            !headers.TryGetValues("X-RateLimit-Reset", out IEnumerable<string>? rawReset)
            && !double.TryParse(rawReset?.SingleOrDefault(), out reset)
        )
        {
            return new ArgumentError("response.Headers", "Failed to parse an X-RateLimit-Reset header from the response.");
        }

        Context? context = request.GetPolicyExecutionContext();

        string route = context is not null && context.TryGetValue("route", out object rawRoute) && rawRoute is string contextRoute
            ? contextRoute
            : $"{request.Method.ToString().ToUpperInvariant()} {request.RequestUri}";

        // if we have a ratelimit, try to encode the result into the preemptive handlers if necessary
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            if
            (
                context is not null
                && context.TryGetValue("simple-route", out object rawSimpleRoute)
                && rawSimpleRoute is SimpleRatelimitRoute simpleRoute
                && simpleRoute.Resource == TopLevelResource.Webhook
            )
            {
                this.webhook429s[simpleRoute.Id] = (float)(reset - this.timeReferencePoint);
            }
            else
            {
                // what if we don't know this route? check whether we know related routes, answer heuristically based on that
                int index = route.IndexOf(' ', StringComparison.Ordinal);
                StringSegment segment = new(route, index + 1, route.Length - index - 1);

                this.route429s[segment] = (float)(reset - this.timeReferencePoint);
            }
        }

        // update the bucket, first by extracting the other relevant information...
        if (!headers.TryGetValues("X-RateLimit-Bucket", out IEnumerable<string>? rawBucket))
        {
            return new ArgumentError("response.Headers", "Failed to parse an X-RateLimit-Bucket header from the response.");
        }

        string hash = rawBucket.Single();
        short limit = 0, remaining = 0;

        if
        (
            !headers.TryGetValues("X-RateLimit-Limit", out IEnumerable<string>? rawLimit)
            && !short.TryParse(rawLimit?.SingleOrDefault(), out limit)
        )
        {
            return new ArgumentError("response.Headers", "Failed to parse an X-RateLimit-Limit header from the response.");
        }

        if
        (
            !headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string>? rawRemaining)
            && !short.TryParse(rawRemaining?.SingleOrDefault(), out remaining)
        )
        {
            return new ArgumentError("response.Headers", "Failed to parse an X-RateLimit-Remaining header from the response.");
        }

        this.dirty = true;

        // ... and then by updating the values we know.
        this.hashes[route] = hash;
        RatelimitBucket bucket = this.ratelimitBuckets.GetOrCreateValue(hash);

        bucket.UpdateFromResponse((float)(reset - this.timeReferencePoint), limit, remaining);

        return default;
    }

    public Result CancelRequest(HttpRequestMessage request)
    {
        Context? context = request.GetPolicyExecutionContext();

        string route = context is not null && context.TryGetValue("route", out object rawRoute) && rawRoute is string contextRoute
            ? contextRoute
            : $"{request.Method.ToString().ToUpperInvariant()} {request.RequestUri}";

        if (this.hashes.TryGetValue(route, out string? hash) && this.ratelimitBuckets.TryGetValue(hash, out RatelimitBucket? bucket))
        {
            bucket.CancelReservation();
        }

        return default;
    }

    // logging delegates defined down here

    private static readonly Action<ILogger, string, int, Exception?> initiatingCleanupMessage = LoggerMessage.Define<string, int>
    (
        LogLevel.Trace,
        default,
        "Initiating {Kind}-based heuristic cleanup, current size: {Size}."
    );

    private static readonly Action<ILogger, string, int, Exception?> completedCleanupMessage = LoggerMessage.Define<string, int>
    (
        LogLevel.Trace,
        default,
        "Completed {Kind}-based heuristic cleanup, new size: {Size}."
    );

    private static readonly Action<ILogger, int, Exception?> initiatingHashCleanupMessage = LoggerMessage.Define<int>
    (
        LogLevel.Trace,
        default,
        "Initiating ratelimit hash cleanup, current size: {Size}."
    );

    private static readonly Action<ILogger, int, Exception?> completedHashCleanupMessage = LoggerMessage.Define<int>
    (
        LogLevel.Trace,
        default,
        "Completed ratelimit hash cleanup, new size: {Size}."
    );

    private static readonly Action<ILogger, int, int, Exception?> startingCleanupLoop = LoggerMessage.Define<int, int>
    (
        LogLevel.Debug,
        default,
        "Starting heuristic cleanup loop with an interval of {Interval}ms and a bucket decay of {Decay}s."
    );

    private static readonly Action<ILogger, string, string, Exception?> rejectedRatelimit = LoggerMessage.Define<string, string>
    (
        LogLevel.Debug,
        default,
        "Rejected request to {Url}, {Status}."
    );
}
