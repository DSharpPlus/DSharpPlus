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
using System.Threading;

namespace DSharpPlus.Core.Rest.Ratelimiting
{
    // keeps track of one single ratelimit for us
    internal record RatelimitBucket
    {
        // default values for if we cant rely on discord
        private const string UnlimitedRatelimitIdentifier = "unlimited";
        private const int DefaultLimit = 1;
        private const int DefaultRemaining = 0;
        private static readonly DateTimeOffset discordEpoch = new(2015, 0, 0, 0, 0, 0, new());

        // lock
        private readonly object __lock = new();

        // the limit for this bucket. this should never change without the hash also changing and us therefore creating a new bucket.
        public int Limit { get; } = DefaultLimit;

#pragma warning disable CS0420 // yes, we are passing ref which makes the reference non-volatile, but you're supposed to use the API like that
        public int Remaining
        {
            get => Volatile.Read(ref _remaining);
            set => Volatile.Write(ref _remaining, value);
        }
#pragma warning restore CS0420

        private volatile int _remaining = DefaultRemaining;

        // this can and will wildly change
        public DateTimeOffset ResetTime { get; private set; } = discordEpoch;

        // and this is what we use to keep track of buckets
        public string Hash { get; } = UnlimitedRatelimitIdentifier;

        #region Constructing

        public RatelimitBucket(int limit, int remaining, DateTimeOffset resetTime, string? hash)
        {
            Limit = limit;
            Remaining = remaining;
            ResetTime = resetTime;
            Hash = hash ?? UnlimitedRatelimitIdentifier;
        }

        public static bool TryExtractRatelimitBucket
        (
            HttpResponseHeaders headers,
    
            [NotNullWhen(true)]
            out RateLimitBucket? bucket
        )
        {
            bucket = null;

            try
            {
                // these three headers are critical for us. if they don't exist, assume no valid ratelimit is passed
                // and abort the operation, indicating that no ratelimit was extracted successfully.

               if
               (
                !headers.TryGetValues("X-RateLimit-Limit", out IEnumerable<string>? limitRaw)          ||
                !headers.TryGetValues("X-RateLimit-Reset", out IEnumerable<string>? resetRaw) ||
                !headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string>? remainingRaw)
               )
                {
                    return false;
                }

                // again, these headers are critical. if they don't hold valid values, abort as well.

                if
                (
                 !int.TryParse(limitRaw.SingleOrDefault(), out int limit)       ||
                 !double.TryParse(resetRaw.SingleOrDefault(), out double reset) ||
                 !int.TryParse(remainingRaw.SingleOrDefault(), out int remaining)
                )
                {
                    return false;
                }

                string? hash = headers.GetValues("X-RateLimit-Name").SingleOrDefault();
                DateTimeOffset resetTime = DateTimeOffset.UnixEpoch + TimeSpan.FromSeconds(reset);

                bucket = new(limit, remaining, resetTime, hash);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        public void ResetBucket(DateTimeOffset nextResetTime)
        {
            if(nextResetTime < ResetTime)
            {
                throw new ArgumentOutOfRangeException(nameof(nextResetTime), "The next ratelimit bucket expiration cannot be in the past.");
            }

            lock (__lock)
            {
                Remaining = Limit;
                ResetTime = nextResetTime;
            }
        }

        public bool AllowNextRequest()
        {
            if(Remaining <= 0)
            {
                // if the bucket should have reset by now, allow it anyway
                return ResetTime < DateTimeOffset.UtcNow;
            }

            Remaining--;
            return true;        
        }
    }
}
