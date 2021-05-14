// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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
using System.Linq;

namespace DSharpPlus
{
    internal class QueryUriBuilder
    {
        public Uri SourceUri { get; }

        public IReadOnlyList<KeyValuePair<string, string>> QueryParameters => this._queryParams;
        private readonly List<KeyValuePair<string, string>> _queryParams = new();

        public QueryUriBuilder(string uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            this.SourceUri = new Uri(uri);
        }

        public QueryUriBuilder(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            this.SourceUri = uri;
        }

        public QueryUriBuilder AddParameter(string key, string value)
        {
            this._queryParams.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        public Uri Build()
        {
            return new UriBuilder(this.SourceUri)
            {
                Query = string.Join("&", this._queryParams.Select(e => Uri.EscapeDataString(e.Key) + '=' + Uri.EscapeDataString(e.Value)))
            }.Uri;
        }

        public override string ToString() => this.Build().ToString();
    }
}
