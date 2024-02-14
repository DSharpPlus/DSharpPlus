using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Exceptions;

using Microsoft.Extensions.Logging;

using Polly;

namespace DSharpPlus.Net;

/// <summary>
/// Represents a client used to make REST requests.
/// </summary>
internal sealed partial class RestClient : IDisposable
{
    [GeneratedRegex(":([a-z_]+)")]
    private static partial Regex GenerateRouteArgumentRegex();

    private static readonly Regex routeArgumentRegex = GenerateRouteArgumentRegex();
    private readonly HttpClient httpClient;
    private readonly ILogger logger;
    private readonly AsyncManualResetEvent globalRateLimitEvent;
    private readonly ResiliencePipeline<HttpResponseMessage> pipeline;

    private volatile bool _disposed;

    internal RestClient(DiscordConfiguration config, ILogger logger)
        : this
        (
            config.Proxy,
            config.HttpTimeout,
            logger,
            config.MaximumRatelimitRetries,
            config.RatelimitRetryDelayFallback,
            config.TimeoutForInitialApiRequest
        )
    {
        this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(config));
        this.httpClient.BaseAddress = new(Endpoints.BASE_URI);
    }

    // This is for meta-clients, such as the webhook client
    internal RestClient
    (
        IWebProxy proxy,
        TimeSpan timeout,
        ILogger logger,
        int maxRetries = -1,
        double retryDelayFallback = 2.5,
        int waitingForHashMilliseconds = 200
    )
    {
        this.logger = logger;

        HttpClientHandler httphandler = new()
        {
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            UseProxy = proxy != null,
            Proxy = proxy
        };

        this.httpClient = new HttpClient(httphandler)
        {
            BaseAddress = new Uri(Utilities.GetApiBaseUri()),
            Timeout = timeout
        };

        this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
        this.httpClient.BaseAddress = new(Endpoints.BASE_URI);

        this.globalRateLimitEvent = new AsyncManualResetEvent(true);

        ResiliencePipelineBuilder<HttpResponseMessage> builder = new();

        builder.AddRetry
        (
            new()
            {
                ShouldHandle = result => ValueTask.FromResult
                (
                        ((maxRetries == -1) || (maxRetries >= result.AttemptNumber))
                        && result.Outcome.Result is not null
                        && (result.Outcome.Result.Headers.Any(xm => xm.Key == "DSharpPlus-Internal-Response")
                        || result.Outcome.Result.StatusCode == HttpStatusCode.TooManyRequests)
                ),
                DelayGenerator = result =>
                {
                    if (result.Outcome.Result!.Headers.TryGetValues("X-RateLimit-Reset-After", out IEnumerable<string>? values))
                    {
                        if (double.TryParse(values.SingleOrDefault(), out double value))
                        {
                            return ValueTask.FromResult<TimeSpan?>(TimeSpan.FromSeconds(value));
                        }
                    }

                    if (result.Outcome.Result!.Headers.RetryAfter?.Delta is not null)
                    {
                        return ValueTask.FromResult(result.Outcome.Result!.Headers.RetryAfter.Delta);
                    }

                    return ValueTask.FromResult<TimeSpan?>(TimeSpan.FromSeconds(retryDelayFallback));
                }
            }
        )
        .AddStrategy(_ => new RateLimitStrategy(logger, waitingForHashMilliseconds), new RateLimitOptions());

        this.pipeline = builder.Build();
    }

    internal async ValueTask<RestResponse> ExecuteRequestAsync<TRequest>
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
            await this.globalRateLimitEvent.WaitAsync();

            ResilienceContext context = ResilienceContextPool.Shared.Get();

            context.Properties.Set(new("route"), request.Route);
            context.Properties.Set(new("exempt-from-global-limit"), request.IsExemptFromGlobalLimit);
            
            using HttpResponseMessage response = await this.pipeline.ExecuteAsync
            (
                async (_) =>
                {
                    using HttpRequestMessage req = request.Build();
                    return await this.httpClient.SendAsync
                    (
                        req,
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None
                    );
                },
                context
            );

            ResilienceContextPool.Shared.Return(context);

            string content = await response.Content.ReadAsStringAsync();

            // consider logging headers too
            this.logger.LogTrace(LoggerEvents.RestRx, "{content}", content);

            _ = response.StatusCode switch
            {
                HttpStatusCode.BadRequest or HttpStatusCode.MethodNotAllowed =>
                    throw new BadRequestException(request.Build(), response, content),

                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                    throw new UnauthorizedException(request.Build(), response, content),

                HttpStatusCode.NotFound =>
                    throw new NotFoundException(request.Build(), response, content),

                HttpStatusCode.RequestEntityTooLarge =>
                    throw new RequestSizeException(request.Build(), response, content),

                HttpStatusCode.TooManyRequests =>
                   throw new RateLimitException(request.Build(), response, content),

                HttpStatusCode.InternalServerError
                    or HttpStatusCode.BadGateway
                    or HttpStatusCode.ServiceUnavailable
                    or HttpStatusCode.GatewayTimeout =>
                    throw new ServerErrorException(request.Build(), response, content),

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
            this.logger.LogError
            (
                LoggerEvents.RestError,
                ex,
                "Request to {url} triggered an exception",
                $"{Endpoints.BASE_URI}/{request.Url}"
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

        this.globalRateLimitEvent.Reset();

        try
        {
            this.httpClient?.Dispose();
        }
        catch { }
    }
}

//       More useless comments, sorry..
//  Was listening to this, felt like sharing.
// https://www.youtube.com/watch?v=ePX5qgDe9s4
//         ♫♪.ılılıll|̲̅̅●̲̅̅|̲̅̅=̲̅̅|̲̅̅●̲̅̅|llılılı.♫♪
