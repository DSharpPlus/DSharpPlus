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

using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Wrap;

namespace DSharpPlus.Core.Rest
{
    /// <summary>
    /// Represents a client for all rest requests to Discord.
    /// </summary>
    public class RestClient
    {
        private readonly IDistributedCache __cache;
        private readonly HttpClient __http_client;
        private readonly AsyncPolicyWrap<HttpResponseMessage>? __wrapped_policy;

        public RestClient(IDistributedCache cache, HttpClient client, int internalRequeues)
        { 
            __cache = cache;
            __http_client = client;

            __wrapped_policy = Policy.WrapAsync(new PollyRatelimitPolicy(), new PollyRetryPolicy(internalRequeues).RetryPolicy);
        }

        public RestClient()
        {
            __cache = new MemoryDistributedCache((IOptions<MemoryDistributedCacheOptions>)new MemoryDistributedCacheOptions()
            {
                // cache options, to be discussed
            });

            __http_client = new();

            // default value to be discussed
            __wrapped_policy = Policy.WrapAsync(new PollyRatelimitPolicy(), new PollyRetryPolicy(1).RetryPolicy);
        }

        public async Task<HttpResponseMessage> MakeRequestAsync(IRestRequest request)
        {
            Context requestContext = new()
            {
                ["cache"] = __cache,
                ["endpoint"] = request.Endpoint,
                ["subject-to-global-limit"] = request.IsSubjectToGlobalLimit
            };

            HttpRequestMessage requestMessage = request.Build();

            requestMessage.SetPolicyExecutionContext(requestContext);

            HttpResponseMessage? response = await __wrapped_policy?.ExecuteAsync(() => __http_client.SendAsync(requestMessage));

            // TODO: error handling

            return response;
        }
    }
}
