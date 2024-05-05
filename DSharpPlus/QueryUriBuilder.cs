using System;
using System.Collections.Generic;
using System.Linq;

namespace DSharpPlus;

internal class QueryUriBuilder
{
    public string SourceUri { get; }

    public IReadOnlyList<KeyValuePair<string, string>> QueryParameters => this.queryParams;
    private readonly List<KeyValuePair<string, string>> queryParams = [];

    public QueryUriBuilder(string uri)
    {
        ArgumentNullException.ThrowIfNull(uri, nameof(uri));

        this.SourceUri = uri;
    }

    public QueryUriBuilder AddParameter(string key, string? value)
    {
        if (value is null)
        {
            return this;
        }

        this.queryParams.Add(new KeyValuePair<string, string>(key, value));
        return this;
    }

    public string Build()
    {
        string query = string.Join
        (
            "&",
            this.queryParams.Select
            (
                e => Uri.EscapeDataString(e.Key) + '=' + Uri.EscapeDataString(e.Value)
            )
        );

        return $"{this.SourceUri}?{query}";
    }

    public override string ToString() => Build().ToString();
}
