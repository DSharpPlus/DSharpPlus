// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Exceptions;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Retry;
using Polly.Wrap;

namespace DSharpPlus.Net;

/// <summary>
/// Represents a client used to make REST requests.
/// </summary>
internal sealed partial class RestClient : IDisposable
{

    [GeneratedRegex(":([a-z_]+)")]
    private static partial Regex GenerateRouteArgumentRegex();

    private static Regex RouteArgumentRegex { get; } = GenerateRouteArgumentRegex();
    private HttpClient HttpClient { get; }
    private BaseDiscordClient? Discord { get; }
    private ILogger Logger { get; }
    private AsyncManualResetEvent GlobalRateLimitEvent { get; }

    private AsyncPolicyWrap<HttpResponseMessage> RateLimitPolicy { get; }

    private volatile bool _disposed;

    internal RestClient(BaseDiscordClient client)
        : this
        (
            client.Configuration.Proxy, 
            client.Configuration.HttpTimeout,
            client.Logger
        )
    {
        this.Discord = client;
        this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(client));
    }

    // This is for meta-clients, such as the webhook client
    internal RestClient
    (
        IWebProxy proxy, 
        TimeSpan timeout,
        ILogger logger
    ) 
    {
        this.Logger = logger;

        HttpClientHandler httphandler = new()
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

        this.GlobalRateLimitEvent = new AsyncManualResetEvent(true);

        // retrying forever is rather suboptimal, but it's the old behaviour. We should discuss whether
        // we want to break this.
        AsyncRetryPolicy<HttpResponseMessage> retry = Policy
            .HandleResult<HttpResponseMessage>
            (
                result => result.Headers.Any(xm => xm.Key == "DSharpPlus-Internal-Response")
                          || result.StatusCode == HttpStatusCode.TooManyRequests
            )
            .RetryForeverAsync();

        this.RateLimitPolicy = Policy.WrapAsync(retry, new RateLimitPolicy(logger));
    }

    internal async ValueTask<RestResponse?> ExecuteRequestAsync<TRequest>
    (
        TRequest request
    )
        where TRequest : struct, IRestRequest
    {
        if (this._disposed)
        {
            throw new ObjectDisposedException
            (
                "DSharpPlus Rest Client", 
                "The Rest Client was disposed. No further requests are possible."
            );
        }

        try
        {
            await this.GlobalRateLimitEvent.WaitAsync();

            using HttpRequestMessage req = request.Build();

            Context context = new()
            {
                ["route"] = request.Route,
                ["exempt-from-global-limit"] = request.IsExemptFromGlobalLimit
            };

            using HttpResponseMessage response = await this.RateLimitPolicy.ExecuteAsync
            (
                async (_) => await this.HttpClient.SendAsync
                (
                    req, 
                    HttpCompletionOption.ResponseContentRead, 
                    CancellationToken.None
                ),
                context
            );

            string content = await response.Content.ReadAsStringAsync();

            this.Logger.LogTrace(LoggerEvents.RestRx, content);

            _ = response.StatusCode switch
            {
                HttpStatusCode.BadRequest or HttpStatusCode.MethodNotAllowed =>
                    throw new BadRequestException(req, response, await response.Content.ReadAsStringAsync()),

                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                    throw new UnauthorizedException(req, response, await response.Content.ReadAsStringAsync()),

                HttpStatusCode.NotFound =>
                    throw new NotFoundException(req, response, await response.Content.ReadAsStringAsync()),

                HttpStatusCode.RequestEntityTooLarge =>
                    throw new RequestSizeException(req, response, await response.Content.ReadAsStringAsync()),

                HttpStatusCode.TooManyRequests =>
                   throw new RateLimitException(req, response, await response.Content.ReadAsStringAsync()),

                HttpStatusCode.InternalServerError
                    or HttpStatusCode.BadGateway
                    or HttpStatusCode.ServiceUnavailable
                    or HttpStatusCode.GatewayTimeout =>
                    throw new ServerErrorException(req, response, await response.Content.ReadAsStringAsync()),

                // we need to keep the c# compiler happy, and not all branches can/should throw here.
                _ => 0
            };

            return new RestResponse()
            {
                Response = content,
                ResponseCode = response.StatusCode
            };
        }
        catch (Exception ex)
        {
            this.Logger.LogError
            (
                LoggerEvents.RestError, 
                ex, 
                "Request to {url} triggered an exception", 
                request.Url
            );

            throw;
        }
    }

    public void Dispose()
    {
        if (this._disposed)
        {
            return;
        }

        this._disposed = true;

        this.GlobalRateLimitEvent.Reset();

        try
        {
            this.HttpClient?.Dispose();
        }
        catch { }
    }
}

//       More useless comments, sorry..
//  Was listening to this, felt like sharing.
// https://www.youtube.com/watch?v=ePX5qgDe9s4
//         ♫♪.ılılıll|̲̅̅●̲̅̅|̲̅̅=̲̅̅|̲̅̅●̲̅̅|llılılı.♫♪
