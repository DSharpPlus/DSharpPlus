// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Responses;

/// <summary>
/// Represents a response from <c>POST /guilds/:guild-id/prune</c>
/// </summary>
public readonly record struct BeginGuildPruneResponse
{
    /// <summary>
    /// The amount of pruned members, or <see langword="null"/> if <c>compute_prune_count</c> was set to 
    /// <see langword="false"/>.
    /// </summary>
    public int? Pruned { get; init; }
}
