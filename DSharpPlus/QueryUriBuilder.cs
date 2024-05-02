
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSharpPlus;
internal class QueryUriBuilder
{
    public string SourceUri { get; }

    public IReadOnlyList<KeyValuePair<string, string>> QueryParameters => _queryParams;
    private readonly List<KeyValuePair<string, string>> _queryParams = [];

    public QueryUriBuilder(string uri)
    {
        ArgumentNullException.ThrowIfNull(uri, nameof(uri));

        SourceUri = uri;
    }

    public QueryUriBuilder AddParameter(string key, string? value)
    {
        if (value is null)
        {
            return this;
        }

        _queryParams.Add(new KeyValuePair<string, string>(key, value));
        return this;
    }

    public string Build()
    {
        string query = string.Join
        (
            "&",
            _queryParams.Select
            (
                e => Uri.EscapeDataString(e.Key) + '=' + Uri.EscapeDataString(e.Value)
            )
        );

        return $"{SourceUri}?{query}";
    }

    public override string ToString() => Build().ToString();
}
