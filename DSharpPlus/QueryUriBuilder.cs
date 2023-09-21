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
