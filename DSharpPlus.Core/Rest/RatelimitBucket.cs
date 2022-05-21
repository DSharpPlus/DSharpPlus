// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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
using System.Runtime.CompilerServices;
using System.Threading;

namespace DSharpPlus.Core.Rest
{

    // represents a single ratelimit bucket, against a single endpoint.
    internal record RatelimitBucket
    {
        private const string UnlimitedRatelimitIdentifier = "unlimited";
        private const int DefaultLimit = 1;
        private const int DefaultRemaining = 0;

        // represents the limit for this bucket - the maximum amount of requests this bucket will allow
        public int Limit { get; internal set; } = DefaultLimit;

#pragma warning disable CS0420
        // stores how many requests remain for this ratelimit bucket.
        public int Remaining
        {
            get => Volatile.Read(ref _remaining);
            set => Volatile.Write(ref _remaining, value);
        }
#pragma warning restore CS0420

        private volatile int _remaining = DefaultRemaining;

        // stores the time at which this bucket will reset.
        public DateTimeOffset ResetTime { get; internal set; } = new();

        // stores the bucket hash used for bucket identification
        public string Hash { get; internal set; } = UnlimitedRatelimitIdentifier;

        // if we aren't passed a hash, assume it's unlimited
        public RatelimitBucket(int limit, int remaining, DateTimeOffset reset, string? hash)
        {
            Limit = limit;
            Remaining = remaining;
            ResetTime = reset;
            Hash = hash ?? UnlimitedRatelimitIdentifier;
        }

        // attempts to extract a ratelimit bucket from the given headers.
        // this will fail if:
        // - any of the required headers are not present
        // - any of the required headers are given in wrong format
        public static bool ExtractRatelimitBucket(HttpResponseHeaders headers,

            [NotNullWhen(true)]
            out RatelimitBucket? bucket)
        {
            bucket = null;

            try
            {
                if (!headers.TryGetValues("X-RateLimit-Limit", out IEnumerable<string>? limitRaw)
                    || !headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string>? remainingRaw)
                    || !headers.TryGetValues("X-RateLimit-Reset", out IEnumerable<string>? resetRaw))
                {
                    return false;
                }

                if (!int.TryParse(limitRaw.SingleOrDefault(), out int limit)
                    || !int.TryParse(remainingRaw.SingleOrDefault(), out int remaining)
                    || !double.TryParse(resetRaw.SingleOrDefault(), out double reset))
                {
                    return false;
                }

                if(!headers.TryGetValues("X-RateLimit-Name", out IEnumerable<string>? hash))
                {
                    return false;
                }

                DateTimeOffset resetTime = DateTimeOffset.UnixEpoch + TimeSpan.FromSeconds(reset);

                bucket = new(limit, remaining, resetTime, hash.SingleOrDefault());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryUpdateCurrentBucket(HttpResponseHeaders headers)
        {
            try
            {
                if (!headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string>? remainingRaw)
                    || !headers.TryGetValues("X-RateLimit-Reset", out IEnumerable<string>? resetRaw))
                {
                    return false;
                }

                if (!int.TryParse(remainingRaw.SingleOrDefault(), out int remaining)
                    || !double.TryParse(resetRaw.SingleOrDefault(), out double reset))
                {
                    return false;
                }

                DateTimeOffset resetTime = DateTimeOffset.UnixEpoch + TimeSpan.FromSeconds(reset);

                Remaining = remaining;
                ResetTime = resetTime;
                return true;
            }
            catch
            {
                return false;
            }
        }

        // synchronized, to avoid a lock statement (or a SemaphoreSlim) here.
        // using MethodImplOptions.Synchronized has proven to be the most efficient way.
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ResetBucket(DateTimeOffset nextReset)
        {
            Remaining = Limit;
            ResetTime = nextReset;
        }

        // not synchronized, because Remaining is volatile.
        public bool AllowNextRequest()
        {
            // if none are remaining, return whether the bucket should have reset by now.
            // since we are using IDistributedCache, the bucket may very well have reset already.
            if (Remaining <= 0)
            {
                return ResetTime < DateTimeOffset.UtcNow;
            }

            Remaining--;
            return true;
        }
    }
}
