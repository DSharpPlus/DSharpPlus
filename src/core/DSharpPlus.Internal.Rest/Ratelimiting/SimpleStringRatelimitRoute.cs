// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Rest.Ratelimiting;

/// <summary>
/// Specifies a route likely defined by its resource and a string.
/// </summary>
internal readonly record struct SimpleStringRatelimitRoute : ISimpleRatelimitRoute
{
    public required TopLevelResource Resource { get; init; }

    /// <summary>
    /// Specifies whether this route can fracture. This is the case for all routes with more than one method
    /// pointing to the otherwise same route, and for all routes that use any ID as a top-level parameter.
    /// </summary>
    public required bool IsFracturable { get; init; }

    public required string Route { get; init; }
}
