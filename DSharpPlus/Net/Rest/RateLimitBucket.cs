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
internal record RateLimitBucket : IEquatable<RateLimitBucket>
{
    public string Hash { get; set; } = UNLIMITED_HASH;

    /// <summary>
    /// Gets the number of uses left before pre-emptive rate limit is triggered.
    /// </summary>
    public int Remaining
        => this._remaining;

    /// <summary>
    /// Gets the maximum number of uses within a single bucket.
    /// </summary>
    public int Maximum { get; set; }

    /// <summary>
    /// Gets the timestamp at which the rate limit resets.
    /// </summary>
    public DateTimeOffset Reset { get; internal set; }

    internal volatile int _remaining;

    private const string UNLIMITED_HASH = "unlimited";

    public RateLimitBucket
    (
        int maximum, 
        int remaining, 
        DateTimeOffset reset, 
        string? hash
    )
    {
        this.Maximum = maximum;
        this._remaining = remaining;
        this.Reset = reset;
        this.Hash = hash ?? UNLIMITED_HASH;
    }

    /// <summary>
    /// Resets the bucket to the next reset time.
    /// </summary>
    internal void TryResetLimit(DateTimeOffset nextReset)
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

            string? hash = headers.GetValues("X-RateLimit-Name").SingleOrDefault();
            DateTimeOffset reset = DateTimeOffset.UnixEpoch + TimeSpan.FromSeconds(ratelimitReset);

            bucket = new(limit, remaining, reset, hash);
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
            return this.Reset < DateTimeOffset.UtcNow;
        }

        this._remaining--;
        return true;
    }
}
