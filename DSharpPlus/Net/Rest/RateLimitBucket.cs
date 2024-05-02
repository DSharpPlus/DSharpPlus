
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
        => remaining;

    /// <summary>
    /// Gets the timestamp at which the rate limit resets.
    /// </summary>
    public DateTime Reset { get; internal set; }

    /// <summary>
    /// Gets the maximum number of uses within a single bucket.
    /// </summary>
    public int Maximum => maximum;

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
        Reset = reset;
    }

    public RateLimitBucket()
    {
        maximum = 1;
        remaining = 1;
        Reset = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        reserved = 0;
    }

    /// <summary>
    /// Resets the bucket to the next reset time.
    /// </summary>
    internal void ResetLimit(DateTime nextReset)
    {
        if (nextReset < Reset)
        {
            throw new ArgumentOutOfRangeException
            (
                nameof(nextReset),
                "The next ratelimit expiration must follow the present expiration."
            );
        }

        Interlocked.Exchange(ref remaining, Maximum);
        Reset = nextReset;
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
        if (Reset < DateTime.UtcNow)
        {
            ResetLimit(DateTime.UtcNow + TimeSpan.FromSeconds(1));
            Interlocked.Increment(ref reserved);
            return true;
        }

        if (Remaining - reserved <= 0)
        {
            return false;
        }

        Interlocked.Increment(ref reserved);
        return true;
    }

    internal void UpdateBucket(int maximum, int remaining, DateTime reset)
    {
        Interlocked.Exchange(ref this.maximum, maximum);
        Interlocked.Exchange(ref this.remaining, remaining);

        if (reserved > 0)
        {
            Interlocked.Decrement(ref reserved);
        }

        Reset = reset;
    }

    internal void CancelReservation()
    {
        if (reserved > 0)
        {
            Interlocked.Decrement(ref reserved);
        }
    }

    internal void CompleteReservation()
    {
        if (Reset < DateTime.UtcNow)
        {
            ResetLimit(DateTime.UtcNow + TimeSpan.FromSeconds(1));

            if (reserved > 0)
            {
                Interlocked.Decrement(ref reserved);
            }

            return;
        }

        Interlocked.Decrement(ref remaining);

        if (reserved > 0)
        {
            Interlocked.Decrement(ref reserved);
        }
    }
}
