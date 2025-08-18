using System;

namespace DSharpPlus.Exceptions;

/// <summary>
/// Thrown if a request made to the gateway was ratelimited.
/// </summary>
public sealed class GatewayRatelimitedException : Exception
{
    /// <summary>
    /// Indicates how long to wait until retrying the request.
    /// </summary>
    public TimeSpan RetryAfter { get; private set; }

    public GatewayRatelimitedException(TimeSpan retryAfter) 
        : base("The attempted operation was ratelimited.") 
        => this.RetryAfter = retryAfter;
}
