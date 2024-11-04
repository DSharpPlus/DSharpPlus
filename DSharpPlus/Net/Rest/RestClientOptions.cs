using System;

namespace DSharpPlus.Net;

/// <summary>
/// Represents configuration options passed to DSharpPlus' Rest client.
/// </summary>
public sealed class RestClientOptions
{
    /// <summary>
    /// Sets the timeout for HTTP operations. Set this to <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> 
    /// to never time out. Defaults to 100 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Specifies the maximum amount of retries to attempt when ratelimited. Retries will still try to respect the ratelimit.
    /// </summary>
    /// <remarks>
    /// Setting this value to 0 disables retrying, including on pre-emptive ratelimits. Defaults to <seealso cref="int.MaxValue"/>.
    /// </remarks>
    public int MaximumRatelimitRetries { get; set; } = int.MaxValue;

    /// <summary>
    /// Specifies the delay to use when there was no delay information passed to the rest client. Defaults to 2.5 seconds.
    /// </summary>
    public TimeSpan RatelimitRetryDelayFallback { get; set; } = TimeSpan.FromMilliseconds(2500);

    /// <summary>
    /// Specifies the time we should be waiting for a ratelimit bucket hash to initialize.
    /// </summary>
    public TimeSpan InitialRequestTimeout { get; set; } = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// Specifies the maximum rest requests to attempt concurrently. Defaults to 15.
    /// </summary>
    /// <remarks>
    /// This is a band-aid for large bots and will be removed in a future version. Do not set this value above 50 unless Discord has
    /// approved you for an increase, and only increase it if your bot is flooding many different endpoints on different guilds and
    /// channels. If your bot is heavily flooding very few endpoints, you may even reduce this limit.
    /// </remarks>
    public int MaximumConcurrentRestRequests { get; set; } = 15;
}
