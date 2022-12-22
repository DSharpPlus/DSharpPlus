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

namespace DSharpPlus.Net;

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
    private readonly TimeSpan _bucketCleanupDelay = TimeSpan.FromSeconds(60);
    private volatile bool _cleanerRunning;
    private Task _cleanerTask;
    private volatile bool _disposed;

    internal RestClient(BaseDiscordClient client)
        : this(client.Configuration.Proxy, client.Configuration.HttpTimeout, client.Configuration.UseRelativeRatelimit, client.Logger)
    {
        Discord = client;
        HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(client));
    }

    internal RestClient(IWebProxy proxy, TimeSpan timeout, bool useRelativeRatelimit,
        ILogger logger) // This is for meta-clients, such as the webhook client
    {
        Logger = logger;

        HttpClientHandler httphandler = new HttpClientHandler
        {
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            UseProxy = proxy != null,
            Proxy = proxy
        };

        HttpClient = new HttpClient(httphandler)
        {
            BaseAddress = new Uri(Utilities.GetApiBaseUri()),
            Timeout = timeout
        };

        HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());

        RoutesToHashes = new ConcurrentDictionary<string, string>();
        HashesToBuckets = new ConcurrentDictionary<string, RateLimitBucket>();
        RequestQueue = new ConcurrentDictionary<string, int>();

        GlobalRateLimitEvent = new AsyncManualResetEvent(true);
        UseResetAfter = useRelativeRatelimit;
    }

    public RateLimitBucket GetBucket(RestRequestMethod method, string route, object route_params, out string url)
    {
        IEnumerable<PropertyInfo> rparams_props = route_params.GetType()
            .GetTypeInfo()
            .DeclaredProperties;
        Dictionary<string, string> rparams = new Dictionary<string, string>();
        foreach (PropertyInfo xp in rparams_props)
        {
            object? val = xp.GetValue(route_params);
            if (val is string xs)
            {
                rparams[xp.Name] = xs;
            }
            else if (val is DateTime dt)
            {
                rparams[xp.Name] = dt.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
            }
            else
            {
                rparams[xp.Name] = val is DateTimeOffset dto
                    ? dto.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)
                    : val is IFormattable xf ? xf.ToString(null, CultureInfo.InvariantCulture) : val.ToString();
            }
        }

        string guild_id = rparams.ContainsKey("guild_id") ? rparams["guild_id"] : "";
        string channel_id = rparams.ContainsKey("channel_id") ? rparams["channel_id"] : "";
        string webhook_id = rparams.ContainsKey("webhook_id") ? rparams["webhook_id"] : "";

        // Create a generic route (minus major params) key
        // ex: POST:/channels/channel_id/messages
        string hashKey = RateLimitBucket.GenerateHashKey(method, route);

        // We check if the hash is present, using our generic route (without major params)
        // ex: in POST:/channels/channel_id/messages, out 80c17d2f203122d936070c88c8d10f33
        // If it doesn't exist, we create an unlimited hash as our initial key in the form of the hash key + the unlimited constant
        // and assign this to the route to hash cache
        // ex: this.RoutesToHashes[POST:/channels/channel_id/messages] = POST:/channels/channel_id/messages:unlimited
        string hash = RoutesToHashes.GetOrAdd(hashKey, RateLimitBucket.GenerateUnlimitedHash(method, route));

        // Next we use the hash to generate the key to obtain the bucket.
        // ex: 80c17d2f203122d936070c88c8d10f33:guild_id:506128773926879242:webhook_id
        // or if unlimited: POST:/channels/channel_id/messages:unlimited:guild_id:506128773926879242:webhook_id
        string bucketId = RateLimitBucket.GenerateBucketId(hash, guild_id, channel_id, webhook_id);

        // If it's not in cache, create a new bucket and index it by its bucket id.
        RateLimitBucket bucket = HashesToBuckets.GetOrAdd(bucketId, new RateLimitBucket(hash, guild_id, channel_id, webhook_id));

        bucket.LastAttemptAt = DateTimeOffset.UtcNow;

        // Cache the routes for each bucket so it can be used for GC later.
        if (!bucket.RouteHashes.Contains(bucketId))
        {
            bucket.RouteHashes.Add(bucketId);
        }

        // Add the current route to the request queue, which indexes the amount
        // of requests occurring to the bucket id.
        _ = RequestQueue.TryGetValue(bucketId, out int count);

        // Increment by one atomically due to concurrency
        RequestQueue[bucketId] = Interlocked.Increment(ref count);

        // Start bucket cleaner if not already running.
        if (!_cleanerRunning)
        {
            _cleanerRunning = true;
            _bucketCleanerTokenSource = new CancellationTokenSource();
            _cleanerTask = Task.Run(CleanupBucketsAsync, _bucketCleanerTokenSource.Token);
            Logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task started.");
        }

        url = RouteArgumentRegex.Replace(route, xm => rparams[xm.Groups[1].Value]);
        return bucket;
    }

    public Task ExecuteRequestAsync(BaseRestRequest request)
        => request == null ? throw new ArgumentNullException(nameof(request)) : ExecuteRequestAsync(request, null, null);

    // to allow proper rescheduling of the first request from a bucket
    private async Task ExecuteRequestAsync(BaseRestRequest request, RateLimitBucket bucket, TaskCompletionSource<bool> ratelimitTcs)
    {
        if (_disposed)
        {
            return;
        }

        HttpResponseMessage res = default;

        try
        {
            await GlobalRateLimitEvent.WaitAsync().ConfigureAwait(false);

            if (bucket == null)
            {
                bucket = request.RateLimitBucket;
            }

            if (ratelimitTcs == null)
            {
                ratelimitTcs = await WaitForInitialRateLimit(bucket).ConfigureAwait(false);
            }

            if (ratelimitTcs == null) // ckeck rate limit only if we are not the probe request
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;

                await bucket.TryResetLimitAsync(now).ConfigureAwait(false);

                // Decrement the remaining number of requests as there can be other concurrent requests before this one finishes and has a chance to update the bucket
                if (Interlocked.Decrement(ref bucket._remaining) < 0)
                {
                    Logger.LogDebug(LoggerEvents.RatelimitDiag, "Request for {Bucket} is blocked", bucket.ToString());
                    TimeSpan delay = bucket.Reset - now;
                    DateTimeOffset resetDate = bucket.Reset;

                    if (UseResetAfter)
                    {
                        delay = bucket.ResetAfter.Value;
                        resetDate = bucket.ResetAfterOffset;
                    }

                    if (delay < new TimeSpan(-TimeSpan.TicksPerMinute))
                    {
                        Logger.LogError(LoggerEvents.RatelimitDiag, "Failed to retrieve ratelimits - giving up and allowing next request for bucket");
                        bucket._remaining = 1;
                    }

                    if (delay < TimeSpan.Zero)
                    {
                        delay = TimeSpan.FromMilliseconds(100);
                    }

                    Logger.LogWarning(LoggerEvents.RatelimitPreemptive, "Pre-emptive ratelimit triggered - waiting until {0:yyyy-MM-dd HH:mm:ss zzz} ({1:c}).", resetDate, delay);
                    Task.Delay(delay)
                        .ContinueWith(_ => ExecuteRequestAsync(request, null, null))
                        .LogTaskFault(Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");

                    return;
                }
                Logger.LogDebug(LoggerEvents.RatelimitDiag, "Request for {Bucket} is allowed", bucket.ToString());
            }
            else
            {
                Logger.LogDebug(LoggerEvents.RatelimitDiag, "Initial request for {Bucket} is allowed", bucket.ToString());
            }

            HttpRequestMessage req = BuildRequest(request);
            RestResponse response = new RestResponse();
            try
            {
                if (_disposed)
                {
                    return;
                }

                res = await HttpClient.SendAsync(req, HttpCompletionOption.ResponseContentRead, CancellationToken.None).ConfigureAwait(false);

                byte[] bts = await res.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                string txt = Utilities.UTF8.GetString(bts, 0, bts.Length);

                Logger.LogTrace(LoggerEvents.RestRx, txt);

                response.Headers = res.Headers.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value), StringComparer.OrdinalIgnoreCase);
                response.Response = txt;
                response.ResponseCode = (int)res.StatusCode;
            }
            catch (HttpRequestException httpex)
            {
                Logger.LogError(LoggerEvents.RestError, httpex, "Request to {Url} triggered an HttpException", request.Url);
                request.SetFaulted(httpex);
                FailInitialRateLimitTest(request, ratelimitTcs);
                return;
            }

            UpdateBucket(request, response, ratelimitTcs);

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
                    Handle429(response, out Task? wait, out bool global);
                    if (wait != null)
                    {
                        if (global)
                        {
                            Logger.LogError(LoggerEvents.RatelimitHit, "Global ratelimit hit, cooling down");
                            try
                            {
                                GlobalRateLimitEvent.Reset();
                                await wait.ConfigureAwait(false);
                            }
                            finally
                            {
                                // we don't want to wait here until all the blocked requests have been run, additionally Set can never throw an exception that could be suppressed here
                                _ = GlobalRateLimitEvent.SetAsync();
                            }
                            ExecuteRequestAsync(request, bucket, ratelimitTcs)
                                .LogTaskFault(Logger, LogLevel.Error, LoggerEvents.RestError, "Error while retrying request");
                        }
                        else
                        {
                            Logger.LogError(LoggerEvents.RatelimitHit, "Ratelimit hit, requeueing request to {Url}", request.Url);
                            await wait.ConfigureAwait(false);
                            ExecuteRequestAsync(request, bucket, ratelimitTcs)
                                .LogTaskFault(Logger, LogLevel.Error, LoggerEvents.RestError, "Error while retrying request");
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
            {
                request.SetFaulted(ex);
            }
            else
            {
                request.SetCompleted(response);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(LoggerEvents.RestError, ex, "Request to {Url} triggered an exception", request.Url);

            // if something went wrong and we couldn't get rate limits for the first request here, allow the next request to run
            if (bucket != null && ratelimitTcs != null && bucket._limitTesting != 0)
            {
                FailInitialRateLimitTest(request, ratelimitTcs);
            }

            if (!request.TrySetFaulted(ex))
            {
                throw;
            }
        }
        finally
        {
            res?.Dispose();

            // Get and decrement active requests in this bucket by 1.
            _ = RequestQueue.TryGetValue(bucket.BucketId, out int count);
            RequestQueue[bucket.BucketId] = Interlocked.Decrement(ref count);

            // If it's 0 or less, we can remove the bucket from the active request queue,
            // along with any of its past routes.
            if (count <= 0)
            {
                foreach (string r in bucket.RouteHashes)
                {
                    if (RequestQueue.ContainsKey(r))
                    {
                        _ = RequestQueue.TryRemove(r, out _);
                    }
                }
            }
        }
    }

    private void FailInitialRateLimitTest(BaseRestRequest request, TaskCompletionSource<bool> ratelimitTcs, bool resetToInitial = false)
    {
        if (ratelimitTcs == null && !resetToInitial)
        {
            return;
        }

        RateLimitBucket bucket = request.RateLimitBucket;

        bucket._limitValid = false;
        bucket._limitTestFinished = null;
        bucket._limitTesting = 0;

        //Reset to initial values.
        if (resetToInitial)
        {
            UpdateHashCaches(request, bucket);
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
                    // if we got here when the first request was just finishing, we must not create the waiter task as it would signal ExecuteRequestAsync to bypass rate limiting
                    if (bucket._limitValid)
                    {
                        return null;
                    }

                    // allow exactly one request to go through without having rate limits available
                    TaskCompletionSource<bool> ratelimitsTcs = new TaskCompletionSource<bool>();
                    bucket._limitTestFinished = ratelimitsTcs.Task;
                    return ratelimitsTcs;
                }
            }
            // it can take a couple of cycles for the task to be allocated, so wait until it happens or we are no longer probing for the limits
            Task waitTask = null;
            while (bucket._limitTesting != 0 && (waitTask = bucket._limitTestFinished) == null)
            {
                await Task.Yield();
            }

            if (waitTask != null)
            {
                await waitTask.ConfigureAwait(false);
            }

            // if the request failed and the response did not have rate limit headers we have allow the next request and wait again, thus this is a loop here
        }
        return null;
    }

    private HttpRequestMessage BuildRequest(BaseRestRequest request)
    {
        HttpRequestMessage req = new HttpRequestMessage(new HttpMethod(request.Method.ToString()), request.Url);
        if (request.Headers != null && request.Headers.Any())
        {
            foreach (KeyValuePair<string, string> kvp in request.Headers)
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }
        }

        if (request is RestRequest nmprequest && !string.IsNullOrWhiteSpace(nmprequest.Payload))
        {
            Logger.LogTrace(LoggerEvents.RestTx, nmprequest.Payload);

            req.Content = new StringContent(nmprequest.Payload);
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        if (request is MultipartWebRequest mprequest)
        {
            Logger.LogTrace(LoggerEvents.RestTx, "<multipart request>");
            if (mprequest.Values.TryGetValue("payload_json", out string? payload))
            {
                Logger.LogTrace(LoggerEvents.RestTx, payload);
            }

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

            req.Headers.Add("Connection", "keep-alive");
            req.Headers.Add("Keep-Alive", "600");

            MultipartFormDataContent content = new MultipartFormDataContent(boundary);
            if (mprequest.Values != null && mprequest.Values.Any())
            {
                foreach (KeyValuePair<string, string> kvp in mprequest.Values)
                {
                    content.Add(new StringContent(kvp.Value), kvp.Key);
                }
            }

            if (mprequest.Files != null && mprequest.Files.Any())
            {
                int i = 1;
                foreach (Entities.DiscordMessageFile f in mprequest.Files)
                {
                    StreamContent sc = new StreamContent(f.Stream);

                    if (f.ContentType != null)
                    {
                        sc.Headers.ContentType = new MediaTypeHeaderValue(f.ContentType);
                    }

                    if (f.FileType != null)
                    {
                        f.FileName += '.' + f.FileType;
                    }

                    string count = !mprequest._removeFileCount ? i++.ToString(CultureInfo.InvariantCulture) : string.Empty;

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
        {
            return;
        }

        IReadOnlyDictionary<string, string> hs = response.Headers;

        // handle the wait
        if (hs.TryGetValue("Retry-After", out string? retry_after_raw))
        {
            TimeSpan retry_after = TimeSpan.FromSeconds(int.Parse(retry_after_raw, CultureInfo.InvariantCulture));
            wait_task = Task.Delay(retry_after);
        }

        // check if global b1nzy
        if (hs.TryGetValue("X-RateLimit-Global", out string? isglobal) && isglobal.Equals("true", StringComparison.InvariantCultureIgnoreCase))
        {
            // global
            global = true;
        }
    }

    private void UpdateBucket(BaseRestRequest request, RestResponse response, TaskCompletionSource<bool> ratelimitTcs)
    {
        RateLimitBucket bucket = request.RateLimitBucket;

        if (response.Headers == null)
        {
            if (response.ResponseCode != 429) // do not fail when ratelimit was or the next request will be scheduled hitting the rate limit again
            {
                FailInitialRateLimitTest(request, ratelimitTcs);
            }

            return;
        }

        IReadOnlyDictionary<string, string> hs = response.Headers;

        if (hs.TryGetValue("X-RateLimit-Global", out string? isglobal) && isglobal.Equals("true", StringComparison.InvariantCultureIgnoreCase))
        {
            if (response.ResponseCode != 429)
            {
                FailInitialRateLimitTest(request, ratelimitTcs);
            }

            return;
        }

        bool r1 = hs.TryGetValue("X-RateLimit-Limit", out string? usesmax);
        bool r2 = hs.TryGetValue("X-RateLimit-Remaining", out string? usesleft);
        bool r3 = hs.TryGetValue("X-RateLimit-Reset", out string? reset);
        bool r4 = hs.TryGetValue("X-Ratelimit-Reset-After", out string? resetAfter);
        bool r5 = hs.TryGetValue("X-Ratelimit-Bucket", out string? hash);

        if (!r1 || !r2 || !r3 || !r4)
        {
            //If the limits were determined before this request, make the bucket initial again.
            if (response.ResponseCode != 429)
            {
                FailInitialRateLimitTest(request, ratelimitTcs, ratelimitTcs == null);
            }

            return;
        }

        DateTimeOffset clienttime = DateTimeOffset.UtcNow;
        DateTimeOffset resettime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(double.Parse(reset, CultureInfo.InvariantCulture));
        DateTimeOffset servertime = clienttime;
        if (hs.TryGetValue("Date", out string? raw_date))
        {
            servertime = DateTimeOffset.Parse(raw_date, CultureInfo.InvariantCulture).ToUniversalTime();
        }

        TimeSpan resetdelta = resettime - servertime;
        //var difference = clienttime - servertime;
        //if (Math.Abs(difference.TotalSeconds) >= 1)
        ////    this.Logger.LogMessage(LogLevel.DebugBaseDiscordClient.RestEventId,  $"Difference between machine and server time: {difference.TotalMilliseconds.ToString("#,##0.00", CultureInfo.InvariantCulture)}ms", DateTime.Now);
        //else
        //    difference = TimeSpan.Zero;

        if (request.RateLimitWaitOverride.HasValue)
        {
            resetdelta = TimeSpan.FromSeconds(request.RateLimitWaitOverride.Value);
        }

        DateTimeOffset newReset = clienttime + resetdelta;

        if (UseResetAfter)
        {
            bucket.ResetAfter = TimeSpan.FromSeconds(double.Parse(resetAfter, CultureInfo.InvariantCulture));
            newReset = clienttime + bucket.ResetAfter.Value + (request.RateLimitWaitOverride.HasValue
                ? resetdelta
                : TimeSpan.Zero);
            bucket.ResetAfterOffset = newReset;
        }
        else
        {
            bucket.Reset = newReset;
        }

        int maximum = int.Parse(usesmax, CultureInfo.InvariantCulture);
        int remaining = int.Parse(usesleft, CultureInfo.InvariantCulture);

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
            {
                bucket._nextReset = newReset.UtcTicks;
            }
        }

        UpdateHashCaches(request, bucket, hash);
    }

    private void UpdateHashCaches(BaseRestRequest request, RateLimitBucket bucket, string newHash = null)
    {
        string hashKey = RateLimitBucket.GenerateHashKey(request.Method, request.Route);

        if (!RoutesToHashes.TryGetValue(hashKey, out string? oldHash))
        {
            return;
        }

        // This is an unlimited bucket, which we don't need to keep track of.
        if (newHash == null)
        {
            _ = RoutesToHashes.TryRemove(hashKey, out _);
            _ = HashesToBuckets.TryRemove(bucket.BucketId, out _);
            return;
        }

        // Only update the hash once, due to a bug on Discord's end.
        // This will cause issues if the bucket hashes are dynamically changed from the API while running,
        // in which case, Dispose will need to be called to clear the caches.
        if (bucket._isUnlimited && newHash != oldHash)
        {
            Logger.LogDebug(LoggerEvents.RestHashMover, "Updating hash in {Hash}: \"{OldHash}\" -> \"{NewHash}\"", hashKey, oldHash, newHash);
            string bucketId = RateLimitBucket.GenerateBucketId(newHash, bucket.GuildId, bucket.ChannelId, bucket.WebhookId);

            _ = RoutesToHashes.AddOrUpdate(hashKey, newHash, (key, oldHash) =>
            {
                bucket.Hash = newHash;

                string oldBucketId = RateLimitBucket.GenerateBucketId(oldHash, bucket.GuildId, bucket.ChannelId, bucket.WebhookId);

                // Remove the old unlimited bucket.
                _ = HashesToBuckets.TryRemove(oldBucketId, out _);
                _ = HashesToBuckets.AddOrUpdate(bucketId, bucket, (key, oldBucket) => bucket);

                return newHash;
            });
        }

        return;
    }

    private async Task CleanupBucketsAsync()
    {
        while (!_bucketCleanerTokenSource.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_bucketCleanupDelay, _bucketCleanerTokenSource.Token).ConfigureAwait(false);
            }
            catch { }

            if (_disposed)
            {
                return;
            }

            //Check and clean request queue first in case it wasn't removed properly during requests.
            foreach (string key in RequestQueue.Keys)
            {
                RateLimitBucket? bucket = HashesToBuckets.Values.FirstOrDefault(x => x.RouteHashes.Contains(key));

                if (bucket == null || (bucket != null && bucket.LastAttemptAt.AddSeconds(5) < DateTimeOffset.UtcNow))
                {
                    _ = RequestQueue.TryRemove(key, out _);
                }
            }

            int removedBuckets = 0;
            StringBuilder bucketIdStrBuilder = default;

            foreach (KeyValuePair<string, RateLimitBucket> kvp in HashesToBuckets)
            {
                if (bucketIdStrBuilder == null)
                {
                    bucketIdStrBuilder = new StringBuilder();
                }

                string key = kvp.Key;
                RateLimitBucket value = kvp.Value;

                // Don't remove the bucket if it's currently being handled by the rest client, unless it's an unlimited bucket.
                if (RequestQueue.ContainsKey(value.BucketId) && !value._isUnlimited)
                {
                    continue;
                }

                DateTimeOffset resetOffset = UseResetAfter ? value.ResetAfterOffset : value.Reset;

                // Don't remove the bucket if it's reset date is less than now + the additional wait time, unless it's an unlimited bucket.
                if (resetOffset != null && !value._isUnlimited && (resetOffset > DateTimeOffset.UtcNow || DateTimeOffset.UtcNow - resetOffset < _bucketCleanupDelay))
                {
                    continue;
                }

                _ = HashesToBuckets.TryRemove(key, out _);
                removedBuckets++;
                bucketIdStrBuilder.Append(value.BucketId + ", ");
            }

            if (removedBuckets > 0)
            {
                Logger.LogDebug(LoggerEvents.RestCleaner, "Removed {BucketCount} unused bucket{BucketPlural}: [{BucketId}]", removedBuckets, removedBuckets > 1 ? "s" : string.Empty, bucketIdStrBuilder.ToString().TrimEnd(',', ' '));
            }

            if (HashesToBuckets.Count == 0)
            {
                break;
            }
        }

        if (!_bucketCleanerTokenSource.IsCancellationRequested)
        {
            _bucketCleanerTokenSource.Cancel();
        }

        _cleanerRunning = false;
        Logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task stopped.");
    }

    ~RestClient()
        => Dispose();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        GlobalRateLimitEvent.Reset();

        if (_bucketCleanerTokenSource?.IsCancellationRequested == false)
        {
            _bucketCleanerTokenSource?.Cancel();
            Logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task stopped.");
        }

        try
        {
            _cleanerTask?.Dispose();
            _bucketCleanerTokenSource?.Dispose();
            HttpClient?.Dispose();
        }
        catch { }

        RoutesToHashes.Clear();
        HashesToBuckets.Clear();
        RequestQueue.Clear();
    }
}


//       More useless comments, sorry..
//  Was listening to this, felt like sharing.
// https://www.youtube.com/watch?v=ePX5qgDe9s4
//         ♫♪.ılılıll|̲̅̅●̲̅̅|̲̅̅=̲̅̅|̲̅̅●̲̅̅|llılılı.♫♪
