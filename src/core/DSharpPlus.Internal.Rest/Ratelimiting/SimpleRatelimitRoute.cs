// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Rest.Ratelimiting;

/// <summary>
/// Specifies a route likely defined by its resource and a snowflake.
/// </summary>
internal readonly record struct SimpleRatelimitRoute
{
    public required TopLevelResource Resource { get; init; }

    public required Snowflake Id { get; init; }
}
