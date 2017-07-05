using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Web;

namespace DSharpPlus
{
    /// <summary>
    /// Represents a client used to make REST requests.
    /// </summary>
    public class RestClient
    {
        private static List<RateLimit> _rateLimits = new List<RateLimit>();
        private static UTF8Encoding utf8 = new UTF8Encoding(false);
        private HttpClient _http;
        private DiscordClient _discord;

        public RestClient(DiscordClient client)
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(Utils.GetApiBaseUri(client))
            };
            _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utils.GetUserAgent());
            _http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utils.GetFormattedToken(client));
            _discord = client;
        }

        /// <summary>
        /// Executes a REST request.
        /// </summary>
        /// <param name="request">REST request to execute.</param>
        /// <returns>Request task.</returns>
        public async Task<WebResponse> HandleRequestAsync(IWebRequest request)
        {
            if (request.GetType() == typeof(WebRequest))
            {
                var req = (WebRequest)request;
                await DelayRequest(req);
                switch (req.Method)
                {
                    case HttpRequestMethod.GET:
                    case HttpRequestMethod.DELETE:
                        {
                            return await WithoutPayloadAsync(req);
                        }
                    case HttpRequestMethod.POST:
                    case HttpRequestMethod.PATCH:
                    case HttpRequestMethod.PUT:
                        {
                            return await WithPayloadAsync(req);
                        }
                    default:
                        throw new NotSupportedException("");
                }
            } else if (request.GetType() == typeof(MultipartWebRequest))
                return await WithMultipartPayloadAsync((MultipartWebRequest)request);
            else
                throw new NotSupportedException("No such IWebRequest known!");
        }

        internal async Task<WebResponse> WithoutPayloadAsync(WebRequest request)
        {
            var req = new HttpRequestMessage(new HttpMethod(request.Method.ToString()), request.URL);
            if (request.Headers != null)
                foreach (var kvp in request.Headers)
                    req.Headers.Add(kvp.Key, kvp.Value);

            WebResponse response = new WebResponse();
            try
            {
                var res = await _http.SendAsync(req, HttpCompletionOption.ResponseContentRead);

                var bts = await res.Content.ReadAsByteArrayAsync();
                var txt = utf8.GetString(bts, 0, bts.Length);

                response.Headers = res.Headers.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value));
                response.Response = txt;
                response.ResponseCode = (int)res.StatusCode;
            }
            catch (HttpRequestException)
            {
                return new WebResponse
                {
                    Headers = null,
                    Response = "",
                    ResponseCode = 0
                };
            }

            HandleRateLimit(request, response);

            // Checking for Errors
            switch (response.ResponseCode)
            {
                case 400:
                case 405:
                    throw new BadRequestException(request, response);
                case 401:
                case 403:
                    throw new UnauthorizedException(request, response);
                case 404:
                    throw new NotFoundException(request, response);
                case 429:
                    throw new RateLimitException(request, response);
            }

            return response;
        }

        internal async Task<WebResponse> WithPayloadAsync(WebRequest request)
        {
            var req = new HttpRequestMessage(new HttpMethod(request.Method.ToString()), request.URL);
            if (request.Headers != null)
                foreach (var kvp in request.Headers)
                    req.Headers.Add(kvp.Key, kvp.Value);

            req.Content = new StringContent(request.Payload);
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            WebResponse response = new WebResponse();
            try
            {
                var res = await _http.SendAsync(req, HttpCompletionOption.ResponseContentRead);

                var bts = await res.Content.ReadAsByteArrayAsync();
                var txt = utf8.GetString(bts, 0, bts.Length);

                response.Headers = res.Headers.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value));
                response.Response = txt;
                response.ResponseCode = (int)res.StatusCode;
            }
            catch (HttpRequestException)
            {
                return new WebResponse
                {
                    Headers = null,
                    Response = "",
                    ResponseCode = 0
                };
            }

            HandleRateLimit(request, response);

            // Checking for Errors
            switch (response.ResponseCode)
            {
                case 400:
                case 405:
                    throw new BadRequestException(request, response);
                case 401:
                case 403:
                    throw new UnauthorizedException(request, response);
                case 404:
                    throw new NotFoundException(request, response);
                case 429:
                    throw new RateLimitException(request, response);
            }

            return response;
        }

        internal async Task<WebResponse> WithMultipartPayloadAsync(MultipartWebRequest request)
        {
            var req = new HttpRequestMessage(new HttpMethod(request.Method.ToString()), request.URL);
            if (request.Headers != null)
                foreach (var kvp in request.Headers)
                    req.Headers.Add(kvp.Key, kvp.Value);

                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

                req.Headers.Add("Connection", "keep-alive");
                req.Headers.Add("Keep-Alive", "600");

                var content = new MultipartFormDataContent(boundary);
                if (request.Values != null)
                    foreach (var kvp in request.Values)
                        content.Add(new StringContent(kvp.Value), kvp.Key);
                if (request.Files != null)
                {
                    int i = 1;
                    foreach (var f in request.Files)
                    {
                        content.Add(new StreamContent(f.Value), $"file{i}", f.Key);
                        i++;
                    }
                }

                req.Content = content;

            WebResponse response = new WebResponse();
            try
            {
                var res = await _http.SendAsync(req, HttpCompletionOption.ResponseContentRead);

                var bts = await res.Content.ReadAsByteArrayAsync();
                var txt = utf8.GetString(bts, 0, bts.Length);

                response.Headers = res.Headers.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value));
                response.Response = txt;
                response.ResponseCode = (int)res.StatusCode;
            }
            catch (HttpRequestException)
            {
                return new WebResponse
                {
                    Headers = null,
                    Response = "",
                    ResponseCode = 0
                };
            }

            HandleRateLimit(request, response);

            // Checking for Errors
            switch (response.ResponseCode)
            {
                case 400:
                case 405:
                    throw new BadRequestException(request, response);
                case 401:
                case 403:
                    throw new UnauthorizedException(request, response);
                case 404:
                    throw new NotFoundException(request, response);
                case 429:
                    throw new RateLimitException(request, response);
            }

            return response;
        }

        internal async Task DelayRequest(WebRequest request)
        {
            RateLimit rateLimit = _rateLimits.Find(x => x.Url == request.URL);
            DateTimeOffset time = DateTimeOffset.UtcNow;
            if (rateLimit != null)
            {
                if (rateLimit.UsesLeft == 0 && rateLimit.Reset > time)
                {
                    request.Discord.DebugLogger.LogMessage(LogLevel.Warning, "Internal", $"Rate-limitted. Waiting till {rateLimit.Reset}", DateTime.Now);
                    await Task.Delay((rateLimit.Reset - time));
                }
                else if (rateLimit.UsesLeft == 0 && rateLimit.Reset < time)
                {
                    _rateLimits.Remove(rateLimit);
                }
            }
        }

        internal void HandleRateLimit(IWebRequest request, WebResponse response)
        {
            if (response.Headers == null || !response.Headers.ContainsKey("X-RateLimit-Reset") || !response.Headers.ContainsKey("X-RateLimit-Remaining") || !response.Headers.ContainsKey("X-RateLimit-Limit"))
                return;

            var clienttime = DateTimeOffset.UtcNow;
            var servertime = DateTimeOffset.Parse(response.Headers["Date"]).ToUniversalTime();
            double difference = clienttime.Subtract(servertime).TotalSeconds;
            request.Discord.DebugLogger.LogMessage(LogLevel.Debug, "REST", "Difference between machine and server time in Ms: " + difference, DateTime.Now);

            RateLimit rateLimit = _rateLimits.Find(x => x.Url == request.URL);
            if (rateLimit != null)
            {
                response.Headers.TryGetValue("X-RateLimit-Limit", out var usesmax);
                response.Headers.TryGetValue("X-RateLimit-Remaining", out var usesleft);
                response.Headers.TryGetValue("X-RateLimit-Reset", out var reset);

                rateLimit.Reset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(double.Parse(reset) + difference);
                rateLimit.UsesLeft = int.Parse(usesleft);
                rateLimit.UsesMax = int.Parse(usesmax);
                _rateLimits[_rateLimits.FindIndex(x => x.Url == request.URL)] = rateLimit;
            }
            else
            {
                response.Headers.TryGetValue("X-RateLimit-Limit", out var usesmax);
                response.Headers.TryGetValue("X-RateLimit-Remaining", out var usesleft);
                response.Headers.TryGetValue("X-RateLimit-Reset", out var reset);
                _rateLimits.Add(new RateLimit
                {
                    Reset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(double.Parse(reset) + difference),
                    Url = request.URL,
                    UsesLeft = int.Parse(usesleft),
                    UsesMax = int.Parse(usesmax)
                });
            }
        }
    }
}
