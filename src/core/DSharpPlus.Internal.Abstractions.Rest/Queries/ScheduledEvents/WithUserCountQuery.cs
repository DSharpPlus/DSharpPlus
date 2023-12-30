// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains the query parameters for scheduled events that may be returned with user counts.
/// </summary>
public readonly record struct WithUserCountQuery
{
    /// <summary>
    /// Specifies whether to include user counts in the returned scheduled events.
    /// </summary>
    public bool? WithUserCount { get; init; }
}