using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;

namespace DSharpPlus.Net
{
    /// <summary>
    /// Represents a client used to make REST requests.
    /// </summary>
    internal sealed class RestClient
    {
        private static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);
        private static Regex RouteArgumentRegex { get; } = new Regex(@":([a-z_]+)");

        private BaseDiscordClient Discord { get; }
        private HttpClient HttpClient { get; }
        private HashSet<RateLimitBucket> Buckets { get; }
        private SemaphoreSlim RequestSemaphore { get; }

        internal RestClient(BaseDiscordClient client)
        {
            this.Discord = client;

            this.HttpClient = new HttpClient
            {
                BaseAddress = new Uri(Utilities.GetApiBaseUri())
            };
            this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
            this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(client));

            this.Buckets = new HashSet<RateLimitBucket>();
            this.RequestSemaphore = new SemaphoreSlim(1, 1);
        }

        public RateLimitBucket GetBucket(RestRequestMethod method, string route, object route_params, out string url)
        {
            var rparams = route_params.GetType()
                .GetTypeInfo()
                .DeclaredProperties
                .ToDictionary(xp => xp.Name, xp => xp.GetValue(route_params) as string);

            var guild_id = rparams.ContainsKey("guild_id") ? rparams["guild_id"] : "";
            var channel_id = rparams.ContainsKey("channel_id") ? rparams["channel_id"] : "";

            var id = RateLimitBucket.GenerateId(method, route, guild_id, channel_id);

            RateLimitBucket bucket = null;
            bucket = this.Buckets.FirstOrDefault(xb => xb.BucketId == id);
            if (bucket == null)
            {
                bucket = new RateLimitBucket(method, route, guild_id, channel_id);
                this.Buckets.Add(bucket);
            }

            url = RouteArgumentRegex.Replace(route, xm => $"{rparams[xm.Groups[1].Value]}");
            return bucket;
        }

        public async Task ExecuteRequestAsync(BaseRestRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            await this.RequestSemaphore.WaitAsync();

            var bucket = request.RateLimitBucket;
            var now = DateTimeOffset.UtcNow;
            if (bucket.Remaining <= 0 && bucket.Maximum > 0 && now < bucket.Reset)
            {
                request.Discord.DebugLogger.LogMessage(LogLevel.Warning, "REST", $"Pre-emptive ratelimit triggered, waiting until {bucket.Reset.ToString("yyyy-MM-dd HH:mm:ss zzz")}", DateTime.Now);
                _ = Task.Delay(bucket.Reset - now).ContinueWith(t => this.ExecuteRequestAsync(request));
                this.RequestSemaphore.Release();
                return;
            }

            var req = this.BuildRequest(request);
            var response = new RestResponse();
            try
            {
                var res = await HttpClient.SendAsync(req, HttpCompletionOption.ResponseContentRead);

                var bts = await res.Content.ReadAsByteArrayAsync();
                var txt = UTF8.GetString(bts, 0, bts.Length);

                response.Headers = res.Headers.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value));
                response.Response = txt;
                response.ResponseCode = (int)res.StatusCode;
            }
            catch (HttpRequestException httpex)
            {
                request.Discord.DebugLogger.LogMessage(LogLevel.Error, "REST", $"Request to {request.Url} triggered an HttpException: {httpex.Message}", DateTime.Now);
                request.SetFaulted(httpex);
                this.RequestSemaphore.Release();
                return;
            }

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

                case 429:
                    ex = new RateLimitException(request, response);

                    // check the limit info, if more than one minute, fault, otherwise requeue
                    this.Handle429(response, out var wait, out var global);
                    if (wait != null)
                    {
                        wait = wait.ContinueWith(t => this.ExecuteRequestAsync(request));
                        if (global)
                        {
                            request.Discord.DebugLogger.LogMessage(LogLevel.Error, "REST", $"Global ratelimit hit, cooling down", DateTime.Now);
                            await wait;
                        }
                        else
                            request.Discord.DebugLogger.LogMessage(LogLevel.Error, "REST", $"Ratelimit hit, requeueing request to {request.Url}", DateTime.Now);
                        this.RequestSemaphore.Release();
                        return;
                    }
                    break;
            }

            this.UpdateBucket(request, response);
            this.RequestSemaphore.Release();

            if (ex != null)
                request.SetFaulted(ex);
            else
                request.SetCompleted(response);
        }

        private HttpRequestMessage BuildRequest(BaseRestRequest request)
        {
            var req = new HttpRequestMessage(new HttpMethod(request.Method.ToString()), request.Url);
            if (request.Headers != null && request.Headers.Any())
                foreach (var kvp in request.Headers)
                    req.Headers.Add(kvp.Key, kvp.Value);

            if (request is RestRequest nmprequest && !string.IsNullOrWhiteSpace(nmprequest.Payload))
            {
                req.Content = new StringContent(nmprequest.Payload);
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            if (request is MultipartWebRequest mprequest)
            {
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

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
                        content.Add(new StreamContent(f.Value), $"file{i++}", f.Key);
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

            // check if global b1nzy
            if (hs.TryGetValue("X-RateLimit-Global", out var isglobal) && isglobal.ToLowerInvariant() == "true")
            {
                // global

                hs.TryGetValue("Retry-After", out var retry_after_raw);
                var retry_after = int.Parse(retry_after_raw);

                // handle the wait
                wait_task = Task.Delay(retry_after);
                global = true;
                return;
            }
        }

        private void UpdateBucket(BaseRestRequest request, RestResponse response)
        {
            if (response.Headers == null)
                return;
            var hs = response.Headers;

            var bucket = request.RateLimitBucket;

            if (hs.TryGetValue("X-RateLimit-Global", out var isglobal) && isglobal.ToLowerInvariant() == "true")
                return;

            var r1 = hs.TryGetValue("X-RateLimit-Limit", out var usesmax);
            var r2 = hs.TryGetValue("X-RateLimit-Remaining", out var usesleft);
            var r3 = hs.TryGetValue("X-RateLimit-Reset", out var reset);

            if (!r1 || !r2 || !r3)
                return;

            var clienttime = DateTimeOffset.UtcNow;
            var servertime = clienttime;
            if (hs.TryGetValue("Date", out var raw_date))
                servertime = DateTimeOffset.Parse(raw_date).ToUniversalTime();

            var difference = clienttime.Subtract(servertime);
            request.Discord.DebugLogger.LogMessage(LogLevel.Debug, "REST", $"Difference between machine and server time: {difference.TotalMilliseconds.ToString("#,##0.00")}ms", DateTime.Now);

            bucket.Maximum = int.Parse(usesmax);
            bucket.Remaining = int.Parse(usesleft);
            bucket.Reset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(long.Parse(reset) + difference.TotalSeconds);
        }
    }
}


//       More useless comments, sorry..
//  Was listening to this, felt like sharing.
// https://www.youtube.com/watch?v=ePX5qgDe9s4
//         ♫♪.ılılıll|̲̅̅●̲̅̅|̲̅̅=̲̅̅|̲̅̅●̲̅̅|llılılı.♫♪