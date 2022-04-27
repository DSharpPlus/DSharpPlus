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

// this is a stub, intended to demonstrate how to set up the rest client rather than
// act as a working client

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using DSharpPlus.Core.Rest;
using DSharpPlus.Core.Rest.Ratelimiting;

using Microsoft.Extensions.DependencyInjection;

using Polly;
using Polly.Contrib.WaitAndRetry;

namespace DSharpPlus.Core
{

    public class DiscordClient
    {
        // unfortunately, we need those two.
        public IServiceCollection ServiceCollection { get; private set; }

        public ServiceProvider Services { get; private set; }

        // this is what we end up using after init has completed.
        public RestClient RestClient { get; private set; }

        // again, one second
        private static readonly TimeSpan oneSecond = TimeSpan.FromSeconds(1);

        public DiscordClient(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;

            // we need to register the rest client via DI to make sure everything works.
            ServiceCollection
                .AddSingleton<RestClient>()
                .AddMemoryCache();

            Services = serviceCollection.BuildServiceProvider();

            #region set up policies, retrying etc. this should be moved to a separate method.
            PollyRatelimitingPolicy ratelimiter = new();

            // these values are arbitrary and should be configurable. they specify how much time should elapse between retries,
            // and how often we should retry. the algorithm creates an exponentially increasing list of delays, with the first one
            // being based on 0.5 seconds here.
            IEnumerable<TimeSpan> retryDelay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(0.5), 10);

            ServiceCollection.AddHttpClient<RestClient>()
                .ConfigureHttpClient((services, httpClient) =>
                {
                    httpClient.BaseAddress = new("https://discord.com/api/v9"); // or whatever, ideally have this in a constant
                    httpClient.DefaultRequestHeaders.UserAgent.Add(new("DSharpPlus", "v5")); // again, constants
                })
                // register our ratelimiter
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(retryDelay).WrapAsync(ratelimiter))

                // register our retrying
                .AddPolicyHandler
                (
                    // only retry 429 in addition to the already specified 5xx and 408
                    Policy.HandleResult<HttpResponseMessage>(result => result.StatusCode == HttpStatusCode.TooManyRequests)
                        .WaitAndRetryAsync(retryCount: 1, // retry once. another arbitrary number.
                        sleepDurationProvider: (_, response, _) =>
                        {
                            HttpResponseMessage message = response.Result;

                            if (message.Headers.GetValues("X-RateLimit-Scope").SingleOrDefault() == "shared")
                            {
                                throw new HttpRequestException("Shared ratelimit hit, not retrying request."); // this should be our own exception type
                            }

                            else if (message == default)
                            {
                                return oneSecond;
                            }
#pragma warning disable IDE0046 // shut up about ternaries
                            else if (message.Headers.RetryAfter == null || message.Headers.RetryAfter.Delta == null)
#pragma warning restore IDE0046
                            {
                                return oneSecond;
                            }
                            else 
                            { 
                                return message.Headers.RetryAfter.Delta.Value; 
                            }
                        },
                        onRetryAsync: (_, _, _, _) => Task.CompletedTask)
                );
            #endregion

            // get the final rest client instance via DI.
            RestClient = Services.GetRequiredService<RestClient>();
        }
    }
}
