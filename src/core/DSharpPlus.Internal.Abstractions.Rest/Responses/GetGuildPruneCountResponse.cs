// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Responses;

/// <summary>
/// Represents a response from <c>GET /guilds/:guild-id/prune</c>
/// </summary>
public readonly record struct GetGuildPruneCountResponse
{
    /// <summary>
    /// The amount of members that would be pruned in this operation.
    /// </summary>
    public int Pruned { get; init; }
}
