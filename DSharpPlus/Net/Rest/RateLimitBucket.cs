using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public int Maximum { get; set; }

    internal int remaining;
    internal int reserved = 0;

    public RateLimitBucket
    (
        int maximum,
        int remaining,
        DateTime reset
    )
    {
        this.Maximum = maximum;
        this.remaining = remaining;
        this.Reset = reset;
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

        this.remaining = this.Maximum;
        this.Reset = nextReset;
    }

    public static bool TryExtractRateLimitBucket
    (
        HttpResponseHeaders headers,

        [NotNullWhen(true)]
        out RateLimitBucket? bucket
    )
    {
        bucket = null!;

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
        if (this.Remaining - this.reserved <= 0)
        {
            if (this.Reset < DateTime.UtcNow)
            {
                Interlocked.Exchange(ref this.remaining, this.Maximum);
                Interlocked.Increment(ref this.reserved);
                return true;
            }

            return false;
        }

        Interlocked.Increment(ref this.reserved);
        return true;
    }

    internal void UpdateBucket(int remaining)
    {
        Interlocked.Exchange(ref this.remaining, remaining);
        Interlocked.Decrement(ref this.reserved);
    }

    internal void CancelReservation()
        => Interlocked.Decrement(ref this.reserved);
}
