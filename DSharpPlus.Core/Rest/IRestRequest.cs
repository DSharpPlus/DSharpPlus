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

using Polly;

namespace DSharpPlus.Core.Rest
{
    // another stub.
    public interface IRestRequest
    {
        // the actual request goes here


        // this needs to be populated by the API client. it needs to have a MemoryCache for the entire API client
        // storing ratelimits, specify whether the request should be subject to the global limit and
        // store the endpoint this request is made to. Major route parameters (guild id, channel id, webhook id)
        // should be sent here as number, minor route parameters (message id, for instance) as a global constant representing
        // them, like ":message_id".
        // cache needs to be passed with the key `cache`
        // request/ratelimiting needs to be passed with the key `subject-to-global-limit`
        // endpoint needs to be passed with the key `endpoint`
        // of course, these are all changeable in PollyRatelimitingPolicy.cs
        public Context Context { get; }

        public HttpRequestMessage Build();
    }
}
