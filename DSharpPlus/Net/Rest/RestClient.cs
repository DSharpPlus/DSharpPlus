namespace DSharpPlus.Net;
using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Exceptions;
using DSharpPlus.Metrics;
using Microsoft.Extensions.Logging;

using Polly;

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
    private readonly RateLimitStrategy rateLimitStrategy;
    private readonly RequestMetricsContainer metrics = new();

    private volatile bool _disposed;

    internal RestClient(DiscordConfiguration config, ILogger logger)
        : this
        (
            config.Proxy,
            config.HttpTimeout,
            logger,
            config.MaximumRatelimitRetries,
            config.RatelimitRetryDelayFallback,
            config.TimeoutForInitialApiRequest,
            config.MaximumRestRequestsPerSecond
        )
    {
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(config));
        httpClient.BaseAddress = new(Endpoints.BASE_URI);
    }

    // This is for meta-clients, such as the webhook client
    internal RestClient
    (
        IWebProxy proxy,
        TimeSpan timeout,
        ILogger logger,
        int maxRetries = int.MaxValue,
        double retryDelayFallback = 2.5,
        int waitingForHashMilliseconds = 200,
        int maximumRequestsPerSecond = 15
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

        httpClient = new HttpClient(httphandler)
        {
            BaseAddress = new Uri(Utilities.GetApiBaseUri()),
            Timeout = timeout
        };

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
        httpClient.BaseAddress = new(Endpoints.BASE_URI);

        globalRateLimitEvent = new AsyncManualResetEvent(true);

        rateLimitStrategy = new(logger, waitingForHashMilliseconds, maximumRequestsPerSecond);

        ResiliencePipelineBuilder<HttpResponseMessage> builder = new();

        builder.AddRetry
        (
            new()
            {
                DelayGenerator = result =>
                    ValueTask.FromResult<TimeSpan?>((result.Outcome.Exception as PreemptiveRatelimitException)?.ResetAfter
                        ?? TimeSpan.FromSeconds(retryDelayFallback)),
                MaxRetryAttempts = maxRetries
            }
        )
        .AddStrategy(_ => rateLimitStrategy, new RateLimitOptions());

        pipeline = builder.Build();
    }

    internal async ValueTask<RestResponse> ExecuteRequestAsync<TRequest>
    (
        TRequest request
    )
        where TRequest : struct, IRestRequest
    {
        if (_disposed)
        {
            throw new ObjectDisposedException
            (
                "DSharpPlus Rest Client",
                "The Rest Client was disposed. No further requests are possible."
            );
        }

        try
        {
            await globalRateLimitEvent.WaitAsync();

            Ulid traceId = Ulid.NewUlid();

            ResilienceContext context = ResilienceContextPool.Shared.Get();

            context.Properties.Set(new("route"), request.Route);
            context.Properties.Set(new("exempt-from-global-limit"), request.IsExemptFromGlobalLimit);
            context.Properties.Set(new("trace-id"), traceId);

            using HttpResponseMessage response = await pipeline.ExecuteAsync
            (
                async (_) =>
                {
                    using HttpRequestMessage req = request.Build();
                    return await httpClient.SendAsync
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
            logger.LogTrace(LoggerEvents.RestRx, "Request {TraceId}: {Content}", traceId, content);

            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest or HttpStatusCode.MethodNotAllowed:

                    metrics.RegisterBadRequest();
                    throw new BadRequestException(request.Build(), response, content);

                case HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden:

                    metrics.RegisterForbidden();
                    throw new UnauthorizedException(request.Build(), response, content);

                case HttpStatusCode.NotFound:

                    metrics.RegisterNotFound();
                    throw new NotFoundException(request.Build(), response, content);

                case HttpStatusCode.RequestEntityTooLarge:

                    metrics.RegisterRequestTooLarge();
                    throw new RequestSizeException(request.Build(), response, content);

                case HttpStatusCode.TooManyRequests:

                    metrics.RegisterRatelimitHit(response.Headers);
                    throw new RateLimitException(request.Build(), response, content);

                case HttpStatusCode.InternalServerError
                    or HttpStatusCode.BadGateway
                    or HttpStatusCode.ServiceUnavailable
                    or HttpStatusCode.GatewayTimeout:

                    metrics.RegisterServerError();
                    throw new ServerErrorException(request.Build(), response, content);

                default:

                    metrics.RegisterSuccess();
                    break;
            }

            return new RestResponse()
            {
                Response = content,
                ResponseCode = response.StatusCode
            };
        }
        catch (Exception ex)
        {
            logger.LogError
            (
                LoggerEvents.RestError,
                ex,
                "Request to {url} triggered an exception",
                $"{Endpoints.BASE_URI}/{request.Url}"
            );

            throw;
        }
    }

    /// <summary>
    /// Gets the request metrics, optionally since the last time they were checked.
    /// </summary>
    /// <param name="sinceLastCall">If set to true, this resets the counter. Lifetime metrics are unaffected.</param>
    /// <returns>A snapshot of the rest metrics.</returns>
    public RequestMetricsCollection GetRequestMetrics(bool sinceLastCall = false)
        => sinceLastCall ? metrics.GetTemporalMetrics() : metrics.GetLifetimeMetrics();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        globalRateLimitEvent.Reset();
        rateLimitStrategy.Dispose();

        try
        {
            httpClient?.Dispose();
        }
        catch { }
    }
}

//       More useless comments, sorry..
//  Was listening to this, felt like sharing.
// https://www.youtube.com/watch?v=ePX5qgDe9s4
//         ♫♪.ılılıll|̲̅̅●̲̅̅|̲̅̅=̲̅̅|̲̅̅●̲̅̅|llılılı.♫♪
