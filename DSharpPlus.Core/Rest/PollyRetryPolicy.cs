using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DSharpPlus.Core.Rest
{
    internal class PollyRetryPolicy
    {
        private readonly TimeSpan __one_second = TimeSpan.FromSeconds(1);

        public PollyRetryPolicy(int internalRequeues)
            => RetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(response => response.StatusCode == HttpStatusCode.TooManyRequests
                    && response.Headers.GetValues("X-DSharpPlus-Preemptive-Ratelimit").Any())
                    .WaitAndRetryAsync(retryCount: internalRequeues,
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

        public AsyncRetryPolicy<HttpResponseMessage> RetryPolicy { get; private set; }
    }
}
