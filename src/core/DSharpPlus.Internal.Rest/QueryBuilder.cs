// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;

using Bundles;

namespace DSharpPlus.Internal.Rest;

/// <summary>
/// Constructs a new query from the specified parameters.
/// </summary>
internal record struct QueryBuilder
{
    public string RootUri { get; set; }

    public DictionarySlim<string, string> Parameters { get; set; }

    public readonly QueryBuilder AddParameter(string key, string value)
    {
        ref string parameter = ref this.Parameters.GetOrAddValueRef(key);
        parameter = value;
        return this;
    }

    public readonly string Build()
        => this.RootUri + string.Join("&", this.Parameters.Select(e => Uri.EscapeDataString(e.Key) + '=' + Uri.EscapeDataString(e.Value)));
}
