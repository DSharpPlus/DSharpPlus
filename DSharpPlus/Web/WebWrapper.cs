using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
            }else
            {
                return await WithPayloadAsync(request);
            }
        }

        internal static async Task<WebResponse> WithoutPayloadAsync(WebRequest request)
        {
            System.Net.HttpWebRequest httpRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(request.URL);
            httpRequest.Method = request.Method.ToString();
            if (request.Headers != null)
                httpRequest.Headers = request.Headers;

            httpRequest.UserAgent = Utils.GetUserAgent();
            httpRequest.ContentType = "application/json";

            WebResponse response = new WebResponse();
            try
            {
                System.Net.HttpWebResponse httpResponse = (System.Net.HttpWebResponse)await httpRequest.GetResponseAsync();
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
            catch (System.Net.WebException ex)
            {
                System.Net.HttpWebResponse httpResponse = (System.Net.HttpWebResponse)ex.Response;

                if (httpResponse == null)
                    return new WebResponse()
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
            System.Net.HttpWebRequest httpRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(request.URL);
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
                byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

                httpRequest.Method = request.Method.ToString();
                if (request.Headers != null)
                    httpRequest.Headers = request.Headers;

                httpRequest.UserAgent = Utils.GetUserAgent();
                httpRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                httpRequest.KeepAlive = true;

                System.IO.Stream rs = await httpRequest.GetRequestStreamAsync();

                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

                if (request.Values != null)
                {
                    foreach (string key in request.Values.Keys)
                    {
                        await rs.WriteAsync(boundarybytes, 0, boundarybytes.Length);
                        string formitem = string.Format(formdataTemplate, key, request.Values[key]);
                        byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                        await rs.WriteAsync(formitembytes, 0, formitembytes.Length);
                    }
                }
                await rs.WriteAsync(boundarybytes, 0, boundarybytes.Length);

                string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                string header = string.Format(headerTemplate, "file", request.FileName, "image/jpeg");
                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                await rs.WriteAsync(headerbytes, 0, headerbytes.Length);

                FileStream fileStream = new FileStream(request.FilePath, FileMode.Open, FileAccess.Read);
                await fileStream.CopyToAsync(rs);

                byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
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
                System.Net.HttpWebResponse httpResponse = (System.Net.HttpWebResponse)await httpRequest.GetResponseAsync();
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
            catch (System.Net.WebException ex)
            {
                System.Net.HttpWebResponse httpResponse = (System.Net.HttpWebResponse)ex.Response;

                if (httpResponse == null)
                    return new WebResponse()
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
                    DiscordClient._debugLogger.LogMessage(LogLevel.Warning, $"Rate-limitted. Waiting till {rateLimit.Reset.ToString()}", DateTime.Now);
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

            RateLimit rateLimit = _rateLimits.Find(x => x.Url == request.URL);
            if (rateLimit != null)
            {
                rateLimit.Reset = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(double.Parse(response.Headers.Get("X-RateLimit-Reset")));
                rateLimit.UsesLeft = int.Parse(response.Headers.Get("X-RateLimit-Remaining"));
                rateLimit.UsesMax = int.Parse(response.Headers.Get("X-RateLimit-Limit"));
                _rateLimits[_rateLimits.FindIndex(x => x.Url == request.URL)] = rateLimit;
            }
            else
            {
                _rateLimits.Add(new RateLimit()
                {
                    Reset = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(double.Parse(response.Headers.Get("X-RateLimit-Reset"))),
                    Url = request.URL,
                    UsesLeft = int.Parse(response.Headers.Get("X-RateLimit-Remaining")),
                    UsesMax = int.Parse(response.Headers.Get("X-RateLimit-Limit"))
                });
            }
        }
    }
}
