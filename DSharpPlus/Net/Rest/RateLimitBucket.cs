using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;

namespace DSharpPlus.Net;

/// <summary>
/// Represents a rate limit bucket.
/// </summary>
internal sealed class RateLimitBucket
{
    /// <summary>
    /// Gets the number of uses left before pre-emptive rate limit is triggered.
    /// </summary>
    public int Remaining
        => this.remaining;

    /// <summary>
    /// Gets the timestamp at which the rate limit resets.
    /// </summary>
    public DateTime Reset { get; internal set; }

    /// <summary>
    /// Gets the maximum number of uses within a single bucket.
    /// </summary>
    public int Maximum => this.maximum;

    internal int maximum;
    internal int remaining;
    internal int reserved = 0;

    public RateLimitBucket
    (
        int maximum,
        int remaining,
        DateTime reset
    )
    {
        this.maximum = maximum;
        this.remaining = remaining;
        this.Reset = reset;
    }

    public RateLimitBucket()
    {
        this.maximum = 1;
        this.remaining = 1;
        this.Reset = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        this.reserved = 0;
    }

    /// <summary>
    /// Resets the bucket to the next reset time.
    /// </summary>
    internal void ResetLimit(DateTime nextReset)
    {
        if (nextReset < this.Reset)
        {
            throw new ArgumentOutOfRangeException
            (
                nameof(nextReset),
                "The next ratelimit expiration must follow the present expiration."
            );
        }

        Interlocked.Exchange(ref this.remaining, this.Maximum);
        this.Reset = nextReset;
    }

    public static bool TryExtractRateLimitBucket
    (
        HttpResponseHeaders headers,

        out RateLimitCandidateBucket bucket
    )
    {
        bucket = default;

        try
        {
            if
            (
                !headers.TryGetValues("X-RateLimit-Limit", out IEnumerable<string>? limitRaw)
                || !headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string>? remainingRaw)
                || !headers.TryGetValues("X-RateLimit-Reset-After", out IEnumerable<string>? ratelimitResetRaw)
            )
            {
                return false;
            }

            if
            (
                !int.TryParse(limitRaw.SingleOrDefault(), CultureInfo.InvariantCulture, out int limit)
                || !int.TryParse(remainingRaw.SingleOrDefault(), CultureInfo.InvariantCulture, out int remaining)
                || !double.TryParse(ratelimitResetRaw.SingleOrDefault(), CultureInfo.InvariantCulture, out double ratelimitReset)
            )
            {
                return false;
            }

            DateTime reset = (DateTimeOffset.UtcNow + TimeSpan.FromSeconds(ratelimitReset)).UtcDateTime;

            bucket = new(limit, remaining, reset);
            return true;
        }
        catch
        {
            return false;
        }
    }

    internal bool CheckNextRequest()
    {
        if (this.Reset < DateTime.UtcNow)
        {
            this.ResetLimit(DateTime.UtcNow + TimeSpan.FromSeconds(1));
            Interlocked.Increment(ref this.reserved);
            return true;
        }

        if (this.Remaining - this.reserved <= 0)
        {
            return false;
        }

        Interlocked.Increment(ref this.reserved);
        return true;
    }

    internal void UpdateBucket(int maximum, int remaining, DateTime reset)
    {
        Interlocked.Exchange(ref this.maximum, maximum);
        Interlocked.Exchange(ref this.remaining, remaining);

        if (this.reserved > 0)
        {
            Interlocked.Decrement(ref this.reserved);
        }

        this.Reset = reset;
    }

    internal void CancelReservation()
    {
        if (this.reserved > 0)
        {
            Interlocked.Decrement(ref this.reserved);
        }
    }

    internal void CompleteReservation()
    {
        if (this.Reset < DateTime.UtcNow)
        {
            this.ResetLimit(DateTime.UtcNow + TimeSpan.FromSeconds(1));

            if (this.reserved > 0)
            {
                Interlocked.Decrement(ref this.reserved);
            }

            return;
        }

        Interlocked.Decrement(ref this.remaining); 
        
        if (this.reserved > 0)
        {
            Interlocked.Decrement(ref this.reserved);
        }
    }
}
