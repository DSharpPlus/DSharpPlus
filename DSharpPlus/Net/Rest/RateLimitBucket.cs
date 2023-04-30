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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        if(nextReset < this.Reset)
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
                || !headers.TryGetValues("X-RateLimit-Reset", out IEnumerable<string>? ratelimitResetRaw)
            )
            {
                return false;
            }

            if 
            (
                !int.TryParse(limitRaw.SingleOrDefault(), out int limit)
                || !int.TryParse(remainingRaw.SingleOrDefault(), out int remaining)
                || !double.TryParse(ratelimitResetRaw.SingleOrDefault(), out double ratelimitReset)
            )
            {
                return false;
            }

            DateTime reset = (DateTimeOffset.UnixEpoch + TimeSpan.FromSeconds(ratelimitReset)).UtcDateTime;

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
            return this.Reset < DateTime.UtcNow;
        }

        this._remaining--;
        return true;
    }
}
