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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;

namespace DSharpPlus.Core.Rest
{
    internal class PollyRetryPolicy
    {
        private readonly TimeSpan __first_retry_delay;
        private readonly TimeSpan __one_second = TimeSpan.FromSeconds(1);
        private readonly int __max_retries;

        public PollyRetryPolicy(TimeSpan firstRetryDelay, int maxRetries, int ratelimitedRetries)
        {
            __first_retry_delay = firstRetryDelay;
            __max_retries = maxRetries;

            RetryPolicy = Policy.HandleResult<HttpResponseMessage>(response => response.IsSuccessStatusCode)
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(__first_retry_delay, __max_retries));

            RatelimitedRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                    .WaitAndRetryAsync(retryCount: ratelimitedRetries,
                        sleepDurationProvider: (_, responseTask, _) =>
                        {
                            HttpResponseMessage message = responseTask.Result;

                            if (message.Headers.GetValues("X-RateLimit-Scope").SingleOrDefault() == "shared")
                            {
                                throw new HttpRequestException("Shared ratelimit hit, not retrying request.");
                            }

                            if (message == default)
                            {
                                return __one_second;
                            }

#pragma warning disable IDE0046 // no ternary operators
                            if (message.Headers.RetryAfter == null || message.Headers.RetryAfter.Delta == null)
                            {
                                return __one_second;
                            }
                            else
                            {
                                return message.Headers.RetryAfter.Delta.Value;
                            }
#pragma warning restore IDE0046
                        },
                        onRetryAsync: (_, _, _, _) => Task.CompletedTask);
        }

        public AsyncRetryPolicy<HttpResponseMessage> RetryPolicy { get; private set; }
            
        public AsyncRetryPolicy<HttpResponseMessage> RatelimitedRetryPolicy { get; private set; }
    }
}
