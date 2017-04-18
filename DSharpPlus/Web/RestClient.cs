using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Web;

namespace DSharpPlus
{
    /// <summary>
    /// Represents a client used to make REST requests.
    /// </summary>
    public static class RestClient
    {
        private static List<RateLimit> _rateLimits = new List<RateLimit>();
        private static HttpClient _http;
        private static UTF8Encoding UTF8 = new UTF8Encoding(false);

        static RestClient()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(Utils.GetApiBaseUri())
            };
            _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utils.GetUserAgent());
            _http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utils.GetFormattedToken());
        }

        /// <summary>
        /// Executes a REST request.
        /// </summary>
        /// <param name="request">REST request to execute.</param>
        /// <returns>Request task.</returns>
        public static async Task<WebResponse> HandleRequestAsync(WebRequest request)
        {
            if (request.ContentType == ContentType.Json)
            {
                await DelayRequest(request);
                switch (request.Method)
                {
                    case HttpRequestMethod.GET:
                    case HttpRequestMethod.DELETE:
                        {
                            return await WithoutPayloadAsync(request);
                        }
                    case HttpRequestMethod.POST:
                    case HttpRequestMethod.PATCH:
                    case HttpRequestMethod.PUT:
                        {
                            return await WithPayloadAsync(request);
                        }
                    default:
                        throw new NotSupportedException("");
                }
            }
            return await WithPayloadAsync(request);
        }

        internal static async Task<WebResponse> WithoutPayloadAsync(WebRequest request)
        {
            var req = new HttpRequestMessage(new HttpMethod(request.Method.ToString()), request.URL);
            foreach (var kvp in request.Headers)
                req.Headers.Add(kvp.Key, kvp.Value);

            WebResponse response = new WebResponse();
            try
            {
                var res = await _http.SendAsync(req, HttpCompletionOption.ResponseContentRead);
                response.Headers = res.Headers.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value));
                response.Response = await res.Content.ReadAsStringAsync();
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

        internal static async Task<WebResponse> WithPayloadAsync(WebRequest request)
        {
            var req = new HttpRequestMessage(new HttpMethod(request.Method.ToString()), request.URL);
            foreach (var kvp in request.Headers)
                req.Headers.Add(kvp.Key, kvp.Value);

            if (request.ContentType == ContentType.Json)
            {
                req.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                req.Content = new StringContent(request.Payload);
            }
            else if (request.ContentType == ContentType.Multipart)
            {
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = UTF8.GetBytes("\r\n--" + boundary + "\r\n");

                req.Headers.TryAddWithoutValidation("Content-Type", string.Concat("multipart/form-data; boundary=", boundary));
                req.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                req.Headers.TryAddWithoutValidation("Keep-Alive", "600");

                var ms = new MemoryStream();
                var formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                var headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";

                if (request.Values != null)
                {
                    foreach (string key in request.Values.Keys)
                    {
                        await ms.WriteAsync(boundarybytes, 0, boundarybytes.Length);
                        string formitem = string.Format(formdataTemplate, key, request.Values[key]);
                        byte[] formitembytes = UTF8.GetBytes(formitem);
                        await ms.WriteAsync(formitembytes, 0, formitembytes.Length);
                    }
                }
                await ms.WriteAsync(boundarybytes, 0, boundarybytes.Length);

                string header = string.Format(headerTemplate, "file", request.FileName, "image/jpeg");
                byte[] headerbytes = Encoding.UTF8.GetBytes(header);
                await ms.WriteAsync(headerbytes, 0, headerbytes.Length);

                using (var fileStream = File.OpenRead(request.FilePath))
                {
                    await fileStream.CopyToAsync(ms);

                    byte[] trailer = new UTF8Encoding(false).GetBytes("\r\n--" + boundary + "--\r\n");
                    await ms.WriteAsync(trailer, 0, trailer.Length);
                }

                ms.Seek(0, SeekOrigin.Begin);
                req.Content = new StreamContent(ms);
            }
            else
            {
                throw new NotSupportedException("Content type not supported!");
            }

            WebResponse response = new WebResponse();
            try
            {
                var res = await _http.SendAsync(req);
                response.Headers = res.Headers.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value));
                response.Response = await res.Content.ReadAsStringAsync();
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

        internal static async Task DelayRequest(WebRequest request)
        {
            RateLimit rateLimit = _rateLimits.Find(x => x.Url == request.URL);
            DateTimeOffset time = DateTimeOffset.UtcNow;
            if (rateLimit != null)
            {
                if (rateLimit.UsesLeft == 0 && rateLimit.Reset > time)
                {
                    DiscordClient._debugLogger.LogMessage(LogLevel.Warning, "Internal", $"Rate-limitted. Waiting till {rateLimit.Reset}", DateTime.Now);
                    await Task.Delay((rateLimit.Reset - time));
                }
                else if(rateLimit.UsesLeft == 0 && rateLimit.Reset < time)
                {
                    _rateLimits.Remove(rateLimit);
                }
            }
        }

        internal static void HandleRateLimit(WebRequest request, WebResponse response)
        {
            if (response.Headers == null || response.Headers["X-RateLimit-Reset"] == null || response.Headers["X-RateLimit-Remaining"] == null || response.Headers["X-RateLimit-Limit"] == null)
                return;

            DateTime clienttime = DateTime.UtcNow;
            DateTime servertime = DateTime.Parse(response.Headers["date"]).ToUniversalTime();
            double difference = clienttime.Subtract(servertime).TotalSeconds;
            DiscordClient._debugLogger.LogMessage(LogLevel.Info, "Internal", "Difference between machine and server time in Ms: " + difference, DateTime.Now);

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
