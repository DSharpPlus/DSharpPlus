using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public static class WebWrapper
    {
        public static List<RateLimit> _rateLimits = new List<RateLimit>();

        public static async Task<WebResponse> HandleRequestAsync(WebRequest request)
        {
            if (request.ContentType == ContentType.Json)
            {
                await DelayRequest(request);
                switch (request.Method)
                {
                    case WebRequestMethod.GET:
                    case WebRequestMethod.DELETE:
                        {
                            return await WithoutPayloadAsync(request);
                        }
                    case WebRequestMethod.POST:
                    case WebRequestMethod.PATCH:
                    case WebRequestMethod.PUT:
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
            HttpWebRequest httpRequest = (HttpWebRequest)System.Net.WebRequest.Create(request.URL);
            httpRequest.Method = request.Method.ToString();
            if (request.Headers != null)
                httpRequest.Headers = request.Headers;

            httpRequest.UserAgent = Utils.GetUserAgent();
            httpRequest.ContentType = "application/json";

            WebResponse response = new WebResponse();
            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)await httpRequest.GetResponseAsync();
                if (httpResponse.Headers != null)
                    response.Headers = httpResponse.Headers;
                response.ResponseCode = (int)httpResponse.StatusCode;

                using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    response.Response = await reader.ReadToEndAsync();
                    reader.Close();
                    reader.Dispose();
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse httpResponse = (HttpWebResponse)ex.Response;

                if (httpResponse == null)
                    return new WebResponse
                    {
                        Headers = null,
                        Response = "",
                        ResponseCode = 0
                    };

                if (httpResponse.Headers != null)
                    response.Headers = httpResponse.Headers;
                response.ResponseCode = (int)httpResponse.StatusCode;

                using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    response.Response = reader.ReadToEnd();
                    reader.Close();
                    reader.Dispose();
                }
            }

            HandleRateLimit(request, response);

            return response;
        }

        internal static async Task<WebResponse> WithPayloadAsync(WebRequest request)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)System.Net.WebRequest.Create(request.URL);
            if (request.ContentType == ContentType.Json)
            {
                httpRequest.Method = request.Method.ToString();
                if (request.Headers != null)
                    httpRequest.Headers = request.Headers;

                httpRequest.UserAgent = Utils.GetUserAgent();
                httpRequest.ContentType = "application/json";

                using (StreamWriter writer = new StreamWriter(await httpRequest.GetRequestStreamAsync()))
                {
                    await writer.WriteLineAsync(request.Payload);
                    await writer.FlushAsync();
                    writer.Close();
                    writer.Dispose();
                }
            }
            else if(request.ContentType == ContentType.Multipart)
            {
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

                httpRequest.Method = request.Method.ToString();
                if (request.Headers != null)
                    httpRequest.Headers = request.Headers;

                httpRequest.UserAgent = Utils.GetUserAgent();
                httpRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                httpRequest.KeepAlive = true;

                Stream rs = await httpRequest.GetRequestStreamAsync();

                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

                if (request.Values != null)
                {
                    foreach (string key in request.Values.Keys)
                    {
                        await rs.WriteAsync(boundarybytes, 0, boundarybytes.Length);
                        string formitem = string.Format(formdataTemplate, key, request.Values[key]);
                        byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                        await rs.WriteAsync(formitembytes, 0, formitembytes.Length);
                    }
                }
                await rs.WriteAsync(boundarybytes, 0, boundarybytes.Length);

                string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                string header = string.Format(headerTemplate, "file", request.FileName, "image/jpeg");
                byte[] headerbytes = Encoding.UTF8.GetBytes(header);
                await rs.WriteAsync(headerbytes, 0, headerbytes.Length);

                FileStream fileStream = new FileStream(request.FilePath, FileMode.Open, FileAccess.Read);
                await fileStream.CopyToAsync(rs);

                byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                await rs.WriteAsync(trailer, 0, trailer.Length);
                rs.Close();
            }
            else
            {
                throw new NotSupportedException("Content type not supported!");
            }

            WebResponse response = new WebResponse();
            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)await httpRequest.GetResponseAsync();
                if (httpResponse.Headers != null)
                    response.Headers = httpResponse.Headers;
                response.ResponseCode = (int)httpResponse.StatusCode;

                using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    response.Response = await reader.ReadToEndAsync();
                    reader.Close();
                    reader.Dispose();
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse httpResponse = (HttpWebResponse)ex.Response;

                if (httpResponse == null)
                    return new WebResponse
                    {
                        Headers = null,
                        Response = "",
                        ResponseCode = 0
                    };

                if (httpResponse.Headers != null)
                    response.Headers = httpResponse.Headers;
                response.ResponseCode = (int)httpResponse.StatusCode;

                using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    response.Response = await reader.ReadToEndAsync();
                    reader.Close();
                    reader.Dispose();
                }
            }

            HandleRateLimit(request, response);

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
                rateLimit.Reset = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(double.Parse(response.Headers.Get("X-RateLimit-Reset")) + difference);
                rateLimit.UsesLeft = int.Parse(response.Headers.Get("X-RateLimit-Remaining"));
                rateLimit.UsesMax = int.Parse(response.Headers.Get("X-RateLimit-Limit"));
                _rateLimits[_rateLimits.FindIndex(x => x.Url == request.URL)] = rateLimit;
            }
            else
            {
                _rateLimits.Add(new RateLimit
                {
                    Reset = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(double.Parse(response.Headers.Get("X-RateLimit-Reset")) + difference),
                    Url = request.URL,
                    UsesLeft = int.Parse(response.Headers.Get("X-RateLimit-Remaining")),
                    UsesMax = int.Parse(response.Headers.Get("X-RateLimit-Limit"))
                });
            }
        }
    }
}
