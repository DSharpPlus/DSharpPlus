// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Abstractions.Rest.Responses;

/// <summary>
/// Represents a response from <c>GET /guilds/:guild-id/threads/active</c>.
/// </summary>
public readonly record struct ListActiveGuildThreadsResponse
{
    /// <summary>
    /// The active threads in this guild, sorted by their ID in descending order.
    /// </summary>
    public required IReadOnlyList<IChannel> Threads { get; init; }

    /// <summary>
    /// The thread member objects corresponding to the thread objects.
    /// </summary>
    public required IReadOnlyList<IThreadMember> Members { get; init; }
}
