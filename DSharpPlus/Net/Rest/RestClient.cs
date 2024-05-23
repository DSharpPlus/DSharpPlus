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
    private readonly RateLimitStrategy rateLimitStrategy;
    private readonly RequestMetricsContainer metrics = new();

    private volatile bool disposed;

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
        this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(config));
        this.httpClient.BaseAddress = new(Endpoints.BASE_URI);
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

        this.httpClient = new HttpClient(httphandler)
        {
            BaseAddress = new Uri(Utilities.GetApiBaseUri()),
            Timeout = timeout
        };

        this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
        this.httpClient.BaseAddress = new(Endpoints.BASE_URI);

        this.globalRateLimitEvent = new AsyncManualResetEvent(true);

        this.rateLimitStrategy = new(logger, waitingForHashMilliseconds, maximumRequestsPerSecond);

        ResiliencePipelineBuilder<HttpResponseMessage> builder = new();

        builder.AddRetry
        (
            new()
            {
                DelayGenerator = result =>
                {
                    TimeSpan? delay = (result.Outcome.Exception as PreemptiveRatelimitException)?.ResetAfter;
                    if (delay is null)
                    {
                        Ulid traceId = result.Context.Properties.TryGetValue(new("trace-id"), out Ulid tid) ? tid : Ulid.Empty;
                        this.logger.LogTrace
                        (
                            LoggerEvents.RatelimitDiag,
                            "Request ID:{TraceId}: Retry strategy was triggered by {ExceptionType} instead of PreemptiveRatelimitException. Using fallback delay instead",
                            traceId,
                            result.Outcome.Exception?.GetType()
                        );
                        return ValueTask.FromResult<TimeSpan?>(TimeSpan.FromSeconds(retryDelayFallback));
                    }

                    return ValueTask.FromResult(delay);

                },
                MaxRetryAttempts = maxRetries
            }
        )
        .AddStrategy(_ => this.rateLimitStrategy, new RateLimitOptions());

        this.pipeline = builder.Build();
    }

    internal async ValueTask<RestResponse> ExecuteRequestAsync<TRequest>
    (
        TRequest request
    )
        where TRequest : struct, IRestRequest
    {
        if (this.disposed)
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

            Ulid traceId = Ulid.NewUlid();

            ResilienceContext context = ResilienceContextPool.Shared.Get();

            context.Properties.Set(new("route"), request.Route);
            context.Properties.Set(new("exempt-from-global-limit"), request.IsExemptFromGlobalLimit);
            context.Properties.Set(new("trace-id"), traceId);

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
            this.logger.LogTrace(LoggerEvents.RestRx, "Request {TraceId}: {Content}", traceId, content);

            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest or HttpStatusCode.MethodNotAllowed:

                    this.metrics.RegisterBadRequest();
                    throw new BadRequestException(request.Build(), response, content);

                case HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden:

                    this.metrics.RegisterForbidden();
                    throw new UnauthorizedException(request.Build(), response, content);

                case HttpStatusCode.NotFound:

                    this.metrics.RegisterNotFound();
                    throw new NotFoundException(request.Build(), response, content);

                case HttpStatusCode.RequestEntityTooLarge:

                    this.metrics.RegisterRequestTooLarge();
                    throw new RequestSizeException(request.Build(), response, content);

                case HttpStatusCode.TooManyRequests:

                    this.metrics.RegisterRatelimitHit(response.Headers);
                    throw new RateLimitException(request.Build(), response, content);

                case HttpStatusCode.InternalServerError
                    or HttpStatusCode.BadGateway
                    or HttpStatusCode.ServiceUnavailable
                    or HttpStatusCode.GatewayTimeout:

                    this.metrics.RegisterServerError();
                    throw new ServerErrorException(request.Build(), response, content);

                default:

                    this.metrics.RegisterSuccess();
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

    /// <summary>
    /// Gets the request metrics, optionally since the last time they were checked.
    /// </summary>
    /// <param name="sinceLastCall">If set to true, this resets the counter. Lifetime metrics are unaffected.</param>
    /// <returns>A snapshot of the rest metrics.</returns>
    public RequestMetricsCollection GetRequestMetrics(bool sinceLastCall = false)
        => sinceLastCall ? this.metrics.GetTemporalMetrics() : this.metrics.GetLifetimeMetrics();

    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;

        this.globalRateLimitEvent.Reset();
        this.rateLimitStrategy.Dispose();

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
