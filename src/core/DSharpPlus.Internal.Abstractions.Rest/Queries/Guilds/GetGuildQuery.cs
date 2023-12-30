// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains query parameters for <c>IGuildRestAPI.GetGuildAsync</c>.
/// </summary>
public readonly record struct GetGuildQuery
{
    /// <summary>
    /// Specifies whether the response should include online member counts.
    /// </summary>
    public bool? WithCounts { get; init; }
}