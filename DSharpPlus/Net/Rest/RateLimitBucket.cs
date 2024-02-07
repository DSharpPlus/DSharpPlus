using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;

namespace DSharpPlus.Net;

/// <summary>
/// Represents a rate limit bucket.
/// </summary>

// this is 16 bytes, which ensures that we can always move this within at most six CPU cycles on any
// modern xarch CPU, and eight? cycles on modern aarch CPUs
// do not change the order of properties without first verifying that `sizeof(RateLimitBucket)` remains
// 16. note to testers: this requires an unsafe context for... some reason.
// it is painful to use DateTime over DateTimeOffset here, but DateTimeOffset means double the copy cost.
internal record struct RateLimitBucket
{
    /// <summary>
    /// Gets the number of uses left before pre-emptive rate limit is triggered.
    /// </summary>
    public int Remaining
        => this._remaining;

    /// <summary>
    /// Gets the timestamp at which the rate limit resets.
    /// </summary>
    public DateTime Reset { get; internal set; }

    /// <summary>
    /// Gets the maximum number of uses within a single bucket.
    /// </summary>
    public int Maximum { get; set; }

    internal volatile int _remaining;

    public RateLimitBucket
    (
        int maximum,
        int remaining,
        DateTime reset
    )
    {
        this.Maximum = maximum;
        this._remaining = remaining;
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

        this._remaining = this.Maximum;
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
        if (this.Remaining <= 0)
        {
            if (this.Reset < DateTime.UtcNow)
            {
                this._remaining = this.Maximum - 1;
                return true;
            }

            return false;
        }

        this._remaining--;
        return true;
    }
}
