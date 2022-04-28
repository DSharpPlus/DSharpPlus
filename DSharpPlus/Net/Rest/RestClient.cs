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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Net
{
    /// <summary>
    /// Represents a client used to make REST requests.
    /// </summary>
    internal sealed class RestClient : IDisposable
    {
        private static Regex RouteArgumentRegex { get; } = new Regex(@":([a-z_]+)");
        private HttpClient HttpClient { get; }
        private BaseDiscordClient Discord { get; }
        private ILogger Logger { get; }
        private ConcurrentDictionary<string, string> RoutesToHashes { get; }
        private ConcurrentDictionary<string, RateLimitBucket> HashesToBuckets { get; }
        private ConcurrentDictionary<string, int> RequestQueue { get; }
        private AsyncManualResetEvent GlobalRateLimitEvent { get; }
        private bool UseResetAfter { get; }

        private CancellationTokenSource _bucketCleanerTokenSource;
        private TimeSpan _bucketCleanupDelay = TimeSpan.FromSeconds(60);
        private volatile bool _cleanerRunning;
        private Task _cleanerTask;
        private volatile bool _disposed;

        internal RestClient(BaseDiscordClient client)
            : this(client.Configuration.Proxy, client.Configuration.HttpTimeout, client.Configuration.UseRelativeRatelimit, client.Logger)
        {
            this.Discord = client;
            this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(client));
        }

        internal RestClient(IWebProxy proxy, TimeSpan timeout, bool useRelativeRatelimit,
            ILogger logger) // This is for meta-clients, such as the webhook client
        {
            this.Logger = logger;

            var httphandler = new HttpClientHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseProxy = proxy != null,
                Proxy = proxy
            };

            this.HttpClient = new HttpClient(httphandler)
            {
                BaseAddress = new Uri(Utilities.GetApiBaseUri()),
                Timeout = timeout
            };

            this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());

            this.RoutesToHashes = new ConcurrentDictionary<string, string>();
            this.HashesToBuckets = new ConcurrentDictionary<string, RateLimitBucket>();
            this.RequestQueue = new ConcurrentDictionary<string, int>();

            this.GlobalRateLimitEvent = new AsyncManualResetEvent(true);
            this.UseResetAfter = useRelativeRatelimit;
        }

        public RateLimitBucket GetBucket(RestRequestMethod method, string route, object route_params, out string url)
        {
            var rparams_props = route_params.GetType()
                .GetTypeInfo()
                .DeclaredProperties;
            var rparams = new Dictionary<string, string>();
            foreach (var xp in rparams_props)
            {
                var val = xp.GetValue(route_params);
                if (val is string xs)
                    rparams[xp.Name] = xs;
                else if (val is DateTime dt)
                    rparams[xp.Name] = dt.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
                else if (val is DateTimeOffset dto)
                    rparams[xp.Name] = dto.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
                else rparams[xp.Name] = val is IFormattable xf ? xf.ToString(null, CultureInfo.InvariantCulture) : val.ToString();
            }

            var guild_id = rparams.ContainsKey("guild_id") ? rparams["guild_id"] : "";
            var channel_id = rparams.ContainsKey("channel_id") ? rparams["channel_id"] : "";
            var webhook_id = rparams.ContainsKey("webhook_id") ? rparams["webhook_id"] : "";

            // Create a generic route (minus major params) key
            // ex: POST:/channels/channel_id/messages
            var hashKey = RateLimitBucket.GenerateHashKey(method, route);

            // We check if the hash is present, using our generic route (without major params)
            // ex: in POST:/channels/channel_id/messages, out 80c17d2f203122d936070c88c8d10f33
            // If it doesn't exist, we create an unlimited hash as our initial key in the form of the hash key + the unlimited constant
            // and assign this to the route to hash cache
            // ex: this.RoutesToHashes[POST:/channels/channel_id/messages] = POST:/channels/channel_id/messages:unlimited
            var hash = this.RoutesToHashes.GetOrAdd(hashKey, RateLimitBucket.GenerateUnlimitedHash(method, route));

            // Next we use the hash to generate the key to obtain the bucket.
            // ex: 80c17d2f203122d936070c88c8d10f33:guild_id:506128773926879242:webhook_id
            // or if unlimited: POST:/channels/channel_id/messages:unlimited:guild_id:506128773926879242:webhook_id
            var bucketId = RateLimitBucket.GenerateBucketId(hash, guild_id, channel_id, webhook_id);

            // If it's not in cache, create a new bucket and index it by its bucket id.
            var bucket = this.HashesToBuckets.GetOrAdd(bucketId, new RateLimitBucket(hash, guild_id, channel_id, webhook_id));

            bucket.LastAttemptAt = DateTimeOffset.UtcNow;

            // Cache the routes for each bucket so it can be used for GC later.
            if (!bucket.RouteHashes.Contains(bucketId))
                bucket.RouteHashes.Add(bucketId);

            // Add the current route to the request queue, which indexes the amount
            // of requests occurring to the bucket id.
            _ = this.RequestQueue.TryGetValue(bucketId, out var count);

            // Increment by one atomically due to concurrency
            this.RequestQueue[bucketId] = Interlocked.Increment(ref count);

            // Start bucket cleaner if not already running.
            if (!this._cleanerRunning)
            {
                this._cleanerRunning = true;
                this._bucketCleanerTokenSource = new CancellationTokenSource();
                this._cleanerTask = Task.Run(this.CleanupBucketsAsync, this._bucketCleanerTokenSource.Token);
                this.Logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task started.");
            }

            url = RouteArgumentRegex.Replace(route, xm => rparams[xm.Groups[1].Value]);
            return bucket;
        }

        public Task ExecuteRequestAsync(BaseRestRequest request)
            => request == null ? throw new ArgumentNullException(nameof(request)) : this.ExecuteRequestAsync(request, null, null);

        // to allow proper rescheduling of the first request from a bucket
        private async Task ExecuteRequestAsync(BaseRestRequest request, RateLimitBucket bucket, TaskCompletionSource<bool> ratelimitTcs)
        {
            if (this._disposed)
                return;

            HttpResponseMessage res = default;

            try
            {
                await this.GlobalRateLimitEvent.WaitAsync().ConfigureAwait(false);

                if (bucket == null)
                    bucket = request.RateLimitBucket;

                if (ratelimitTcs == null)
                    ratelimitTcs = await this.WaitForInitialRateLimit(bucket).ConfigureAwait(false);

                if (ratelimitTcs == null) // ckeck rate limit only if we are not the probe request
                {
                    var now = DateTimeOffset.UtcNow;

                    await bucket.TryResetLimitAsync(now).ConfigureAwait(false);

                    // Decrement the remaining number of requests as there can be other concurrent requests before this one finishes and has a chance to update the bucket
                    if (Interlocked.Decrement(ref bucket._remaining) < 0)
                    {
                        this.Logger.LogDebug(LoggerEvents.RatelimitDiag, "Request for {Bucket} is blocked", bucket.ToString());
                        var delay = bucket.Reset - now;
                        var resetDate = bucket.Reset;

                        if (this.UseResetAfter)
                        {
                            delay = bucket.ResetAfter.Value;
                            resetDate = bucket.ResetAfterOffset;
                        }

                        if (delay < new TimeSpan(-TimeSpan.TicksPerMinute))
                        {
                            this.Logger.LogError(LoggerEvents.RatelimitDiag, "Failed to retrieve ratelimits - giving up and allowing next request for bucket");
                            bucket._remaining = 1;
                        }

                        if (delay < TimeSpan.Zero)
                            delay = TimeSpan.FromMilliseconds(100);

                        this.Logger.LogWarning(LoggerEvents.RatelimitPreemptive, "Pre-emptive ratelimit triggered - waiting until {0:yyyy-MM-dd HH:mm:ss zzz} ({1:c}).", resetDate, delay);
                        Task.Delay(delay)
                            .ContinueWith(_ => this.ExecuteRequestAsync(request, null, null))
                            .LogTaskFault(this.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");

                        return;
                    }
                    this.Logger.LogDebug(LoggerEvents.RatelimitDiag, "Request for {Bucket} is allowed", bucket.ToString());
                }
                else
                    this.Logger.LogDebug(LoggerEvents.RatelimitDiag, "Initial request for {Bucket} is allowed", bucket.ToString());

                var req = this.BuildRequest(request);
                var response = new RestResponse();
                try
                {
                    if (this._disposed)
                        return;

                    res = await this.HttpClient.SendAsync(req, HttpCompletionOption.ResponseContentRead, CancellationToken.None).ConfigureAwait(false);

                    var bts = await res.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    var txt = Utilities.UTF8.GetString(bts, 0, bts.Length);

                    this.Logger.LogTrace(LoggerEvents.RestRx, txt);

                    response.Headers = res.Headers.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value), StringComparer.OrdinalIgnoreCase);
                    response.Response = txt;
                    response.ResponseCode = (int)res.StatusCode;
                }
                catch (HttpRequestException httpex)
                {
                    this.Logger.LogError(LoggerEvents.RestError, httpex, "Request to {Url} triggered an HttpException", request.Url);
                    request.SetFaulted(httpex);
                    this.FailInitialRateLimitTest(request, ratelimitTcs);
                    return;
                }

                this.UpdateBucket(request, response, ratelimitTcs);

                Exception ex = null;
                switch (response.ResponseCode)
                {
                    case 400:
                    case 405:
                        ex = new BadRequestException(request, response);
                        break;

                    case 401:
                    case 403:
                        ex = new UnauthorizedException(request, response);
                        break;

                    case 404:
                        ex = new NotFoundException(request, response);
                        break;

                    case 413:
                        ex = new RequestSizeException(request, response);
                        break;

                    case 429:
                        ex = new RateLimitException(request, response);

                        // check the limit info and requeue
                        this.Handle429(response, out var wait, out var global);
                        if (wait != null)
                        {
                            if (global)
                            {
                                this.Logger.LogError(LoggerEvents.RatelimitHit, "Global ratelimit hit, cooling down");
                                try
                                {
                                    this.GlobalRateLimitEvent.Reset();
                                    await wait.ConfigureAwait(false);
                                }
                                finally
                                {
                                    // we don't want to wait here until all the blocked requests have been run, additionally Set can never throw an exception that could be suppressed here
                                    _ = this.GlobalRateLimitEvent.SetAsync();
                                }
                                this.ExecuteRequestAsync(request, bucket, ratelimitTcs)
                                    .LogTaskFault(this.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while retrying request");
                            }
                            else
                            {
                                this.Logger.LogError(LoggerEvents.RatelimitHit, "Ratelimit hit, requeueing request to {Url}", request.Url);
                                await wait.ConfigureAwait(false);
                                this.ExecuteRequestAsync(request, bucket, ratelimitTcs)
                                    .LogTaskFault(this.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while retrying request");
                            }

                            return;
                        }
                        break;

                    case 500:
                    case 502:
                    case 503:
                    case 504:
                        ex = new ServerErrorException(request, response);
                        break;
                }

                if (ex != null)
                    request.SetFaulted(ex);
                else
                    request.SetCompleted(response);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(LoggerEvents.RestError, ex, "Request to {Url} triggered an exception", request.Url);

                // if something went wrong and we couldn't get rate limits for the first request here, allow the next request to run
                if (bucket != null && ratelimitTcs != null && bucket._limitTesting != 0)
                    this.FailInitialRateLimitTest(request, ratelimitTcs);

                if (!request.TrySetFaulted(ex))
                    throw;
            }
            finally
            {
                res?.Dispose();

                // Get and decrement active requests in this bucket by 1.
                _ = this.RequestQueue.TryGetValue(bucket.BucketId, out var count);
                this.RequestQueue[bucket.BucketId] = Interlocked.Decrement(ref count);

                // If it's 0 or less, we can remove the bucket from the active request queue,
                // along with any of its past routes.
                if (count <= 0)
                {
                    foreach (var r in bucket.RouteHashes)
                    {
                        if (this.RequestQueue.ContainsKey(r))
                        {
                            _ = this.RequestQueue.TryRemove(r, out _);
                        }
                    }
                }
            }
        }

        private void FailInitialRateLimitTest(BaseRestRequest request, TaskCompletionSource<bool> ratelimitTcs, bool resetToInitial = false)
        {
            if (ratelimitTcs == null && !resetToInitial)
                return;

            var bucket = request.RateLimitBucket;

            bucket._limitValid = false;
            bucket._limitTestFinished = null;
            bucket._limitTesting = 0;

            //Reset to initial values.
            if (resetToInitial)
            {
                this.UpdateHashCaches(request, bucket);
                bucket.Maximum = 0;
                bucket._remaining = 0;
                return;
            }

            // no need to wait on all the potentially waiting tasks
            _ = Task.Run(() => ratelimitTcs.TrySetResult(false));
        }

        private async Task<TaskCompletionSource<bool>> WaitForInitialRateLimit(RateLimitBucket bucket)
        {
            while (!bucket._limitValid)
            {
                if (bucket._limitTesting == 0)
                {
                    if (Interlocked.CompareExchange(ref bucket._limitTesting, 1, 0) == 0)
                    {
                        // if we got here when the first request was just finishing, we must not create the waiter task as it would signel ExecureRequestAsync to bypass rate limiting
                        if (bucket._limitValid)
                            return null;

                        // allow exactly one request to go through without having rate limits available
                        var ratelimitsTcs = new TaskCompletionSource<bool>();
                        bucket._limitTestFinished = ratelimitsTcs.Task;
                        return ratelimitsTcs;
                    }
                }
                // it can take a couple of cycles for the task to be allocated, so wait until it happens or we are no longer probing for the limits
                Task waitTask = null;
                while (bucket._limitTesting != 0 && (waitTask = bucket._limitTestFinished) == null)
                    await Task.Yield();
                if (waitTask != null)
                    await waitTask.ConfigureAwait(false);

                // if the request failed and the response did not have rate limit headers we have allow the next request and wait again, thus this is a loop here
            }
            return null;
        }

        private HttpRequestMessage BuildRequest(BaseRestRequest request)
        {
            var req = new HttpRequestMessage(new HttpMethod(request.Method.ToString()), request.Url);
            if (request.Headers != null && request.Headers.Any())
                foreach (var kvp in request.Headers)
                    req.Headers.Add(kvp.Key, kvp.Value);

            if (request is RestRequest nmprequest && !string.IsNullOrWhiteSpace(nmprequest.Payload))
            {
                this.Logger.LogTrace(LoggerEvents.RestTx, nmprequest.Payload);

                req.Content = new StringContent(nmprequest.Payload);
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            if (request is MultipartWebRequest mprequest)
            {
                this.Logger.LogTrace(LoggerEvents.RestTx, "<multipart request>");
                if (mprequest.Values.TryGetValue("payload_json", out var payload))
                    this.Logger.LogTrace(LoggerEvents.RestTx, payload);

                var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

                req.Headers.Add("Connection", "keep-alive");
                req.Headers.Add("Keep-Alive", "600");

                var content = new MultipartFormDataContent(boundary);
                if (mprequest.Values != null && mprequest.Values.Any())
                    foreach (var kvp in mprequest.Values)
                        content.Add(new StringContent(kvp.Value), kvp.Key);

                if (mprequest.Files != null && mprequest.Files.Any())
                {
                    var i = 1;
                    foreach (var f in mprequest.Files)
                    {
                        var sc = new StreamContent(f.Stream);

                        if (f.ContentType != null)
                            sc.Headers.ContentType = new MediaTypeHeaderValue(f.ContentType);

                        if (f.FileType != null)
                            f.FileName += '.' + f.FileType;

                        var count = !mprequest._removeFileCount ? i++.ToString(CultureInfo.InvariantCulture) : string.Empty;

                        content.Add(sc, $"file{count}", f.FileName);
                    }
                }

                req.Content = content;
            }

            return req;
        }

        private void Handle429(RestResponse response, out Task wait_task, out bool global)
        {
            wait_task = null;
            global = false;

            if (response.Headers == null)
                return;
            var hs = response.Headers;

            // handle the wait
            if (hs.TryGetValue("Retry-After", out var retry_after_raw))
            {
                var retry_after = TimeSpan.FromSeconds(int.Parse(retry_after_raw, CultureInfo.InvariantCulture));
                wait_task = Task.Delay(retry_after);
            }

            // check if global b1nzy
            if (hs.TryGetValue("X-RateLimit-Global", out var isglobal) && isglobal.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                // global
                global = true;
            }
        }

        private void UpdateBucket(BaseRestRequest request, RestResponse response, TaskCompletionSource<bool> ratelimitTcs)
        {
            var bucket = request.RateLimitBucket;

            if (response.Headers == null)
            {
                if (response.ResponseCode != 429) // do not fail when ratelimit was or the next request will be scheduled hitting the rate limit again
                    this.FailInitialRateLimitTest(request, ratelimitTcs);
                return;
            }

            var hs = response.Headers;

            if (hs.TryGetValue("X-RateLimit-Global", out var isglobal) && isglobal.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                if (response.ResponseCode != 429)
                    this.FailInitialRateLimitTest(request, ratelimitTcs);

                return;
            }

            var r1 = hs.TryGetValue("X-RateLimit-Limit", out var usesmax);
            var r2 = hs.TryGetValue("X-RateLimit-Remaining", out var usesleft);
            var r3 = hs.TryGetValue("X-RateLimit-Reset", out var reset);
            var r4 = hs.TryGetValue("X-Ratelimit-Reset-After", out var resetAfter);
            var r5 = hs.TryGetValue("X-Ratelimit-Bucket", out var hash);

            if (!r1 || !r2 || !r3 || !r4)
            {
                //If the limits were determined before this request, make the bucket initial again.
                if (response.ResponseCode != 429)
                    this.FailInitialRateLimitTest(request, ratelimitTcs, ratelimitTcs == null);

                return;
            }

            var clienttime = DateTimeOffset.UtcNow;
            var resettime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(double.Parse(reset, CultureInfo.InvariantCulture));
            var servertime = clienttime;
            if (hs.TryGetValue("Date", out var raw_date))
                servertime = DateTimeOffset.Parse(raw_date, CultureInfo.InvariantCulture).ToUniversalTime();

            var resetdelta = resettime - servertime;
            //var difference = clienttime - servertime;
            //if (Math.Abs(difference.TotalSeconds) >= 1)
            ////    this.Logger.LogMessage(LogLevel.DebugBaseDiscordClient.RestEventId,  $"Difference between machine and server time: {difference.TotalMilliseconds.ToString("#,##0.00", CultureInfo.InvariantCulture)}ms", DateTime.Now);
            //else
            //    difference = TimeSpan.Zero;

            if (request.RateLimitWaitOverride.HasValue)
                resetdelta = TimeSpan.FromSeconds(request.RateLimitWaitOverride.Value);
            var newReset = clienttime + resetdelta;

            if (this.UseResetAfter)
            {
                bucket.ResetAfter = TimeSpan.FromSeconds(double.Parse(resetAfter, CultureInfo.InvariantCulture));
                newReset = clienttime + bucket.ResetAfter.Value + (request.RateLimitWaitOverride.HasValue
                    ? resetdelta
                    : TimeSpan.Zero);
                bucket.ResetAfterOffset = newReset;
            }
            else
                bucket.Reset = newReset;

            var maximum = int.Parse(usesmax, CultureInfo.InvariantCulture);
            var remaining = int.Parse(usesleft, CultureInfo.InvariantCulture);

            if (ratelimitTcs != null)
            {
                // initial population of the ratelimit data
                bucket.SetInitialValues(maximum, remaining, newReset);

                _ = Task.Run(() => ratelimitTcs.TrySetResult(true));
            }
            else
            {
                // only update the bucket values if this request was for a newer interval than the one
                // currently in the bucket, to avoid issues with concurrent requests in one bucket
                // remaining is reset by TryResetLimit and not the response, just allow that to happen when it is time
                if (bucket._nextReset == 0)
                    bucket._nextReset = newReset.UtcTicks;
            }

            this.UpdateHashCaches(request, bucket, hash);
        }

        private void UpdateHashCaches(BaseRestRequest request, RateLimitBucket bucket, string newHash = null)
        {
            var hashKey = RateLimitBucket.GenerateHashKey(request.Method, request.Route);

            if (!this.RoutesToHashes.TryGetValue(hashKey, out var oldHash))
                return;

            // This is an unlimited bucket, which we don't need to keep track of.
            if (newHash == null)
            {
                _ = this.RoutesToHashes.TryRemove(hashKey, out _);
                _ = this.HashesToBuckets.TryRemove(bucket.BucketId, out _);
                return;
            }

            // Only update the hash once, due to a bug on Discord's end.
            // This will cause issues if the bucket hashes are dynamically changed from the API while running,
            // in which case, Dispose will need to be called to clear the caches.
            if (bucket._isUnlimited && newHash != oldHash)
            {
                this.Logger.LogDebug(LoggerEvents.RestHashMover, "Updating hash in {Hash}: \"{OldHash}\" -> \"{NewHash}\"", hashKey, oldHash, newHash);
                var bucketId = RateLimitBucket.GenerateBucketId(newHash, bucket.GuildId, bucket.ChannelId, bucket.WebhookId);

                _ = this.RoutesToHashes.AddOrUpdate(hashKey, newHash, (key, oldHash) =>
                {
                    bucket.Hash = newHash;

                    var oldBucketId = RateLimitBucket.GenerateBucketId(oldHash, bucket.GuildId, bucket.ChannelId, bucket.WebhookId);

                    // Remove the old unlimited bucket.
                    _ = this.HashesToBuckets.TryRemove(oldBucketId, out _);
                    _ = this.HashesToBuckets.AddOrUpdate(bucketId, bucket, (key, oldBucket) => bucket);

                    return newHash;
                });
            }

            return;
        }

        private async Task CleanupBucketsAsync()
        {
            while (!this._bucketCleanerTokenSource.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(this._bucketCleanupDelay, this._bucketCleanerTokenSource.Token).ConfigureAwait(false);
                }
                catch { }

                if (this._disposed)
                    return;

                //Check and clean request queue first in case it wasn't removed properly during requests.
                foreach (var key in this.RequestQueue.Keys)
                {
                    var bucket = this.HashesToBuckets.Values.FirstOrDefault(x => x.RouteHashes.Contains(key));

                    if (bucket == null || (bucket != null && bucket.LastAttemptAt.AddSeconds(5) < DateTimeOffset.UtcNow))
                        _ = this.RequestQueue.TryRemove(key, out _);
                }

                var removedBuckets = 0;
                StringBuilder bucketIdStrBuilder = default;

                foreach (var kvp in this.HashesToBuckets)
                {
                    if (bucketIdStrBuilder == null)
                        bucketIdStrBuilder = new StringBuilder();

                    var key = kvp.Key;
                    var value = kvp.Value;

                    // Don't remove the bucket if it's currently being handled by the rest client, unless it's an unlimited bucket.
                    if (this.RequestQueue.ContainsKey(value.BucketId) && !value._isUnlimited)
                        continue;

                    var resetOffset = this.UseResetAfter ? value.ResetAfterOffset : value.Reset;

                    // Don't remove the bucket if it's reset date is less than now + the additional wait time, unless it's an unlimited bucket.
                    if (resetOffset != null && !value._isUnlimited && (resetOffset > DateTimeOffset.UtcNow || DateTimeOffset.UtcNow - resetOffset < this._bucketCleanupDelay))
                        continue;

                    _ = this.HashesToBuckets.TryRemove(key, out _);
                    removedBuckets++;
                    bucketIdStrBuilder.Append(value.BucketId + ", ");
                }

                if (removedBuckets > 0)
                    this.Logger.LogDebug(LoggerEvents.RestCleaner, "Removed {BucketCount} unused bucket{BucketPlural}: [{BucketId}]", removedBuckets, removedBuckets > 1 ? "s" : string.Empty, bucketIdStrBuilder.ToString().TrimEnd(',', ' '));

                if (this.HashesToBuckets.Count == 0)
                    break;
            }

            if (!this._bucketCleanerTokenSource.IsCancellationRequested)
                this._bucketCleanerTokenSource.Cancel();

            this._cleanerRunning = false;
            this.Logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task stopped.");
        }

        ~RestClient()
            => this.Dispose();

        public void Dispose()
        {
            if (this._disposed)
                return;

            this._disposed = true;

            this.GlobalRateLimitEvent.Reset();

            if (this._bucketCleanerTokenSource?.IsCancellationRequested == false)
            {
                this._bucketCleanerTokenSource?.Cancel();
                this.Logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task stopped.");
            }

            try
            {
                this._cleanerTask?.Dispose();
                this._bucketCleanerTokenSource?.Dispose();
                this.HttpClient?.Dispose();
            }
            catch { }

            this.RoutesToHashes.Clear();
            this.HashesToBuckets.Clear();
            this.RequestQueue.Clear();
        }
    }
}


//       More useless comments, sorry..
//  Was listening to this, felt like sharing.
// https://www.youtube.com/watch?v=ePX5qgDe9s4
//         ♫♪.ılılıll|̲̅̅●̲̅̅|̲̅̅=̲̅̅|̲̅̅●̲̅̅|llılılı.♫♪
