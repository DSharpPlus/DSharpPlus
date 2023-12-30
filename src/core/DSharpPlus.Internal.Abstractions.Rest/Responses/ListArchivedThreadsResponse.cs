// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Abstractions.Rest.Responses;

/// <summary>
/// Represents the response received from fetching archived threads.
/// </summary>
public readonly record struct ListArchivedThreadsResponse
{
    /// <summary>
    /// The archived threads.
    /// </summary>
    public IReadOnlyList<IChannel> Threads { get; }

    /// <summary>
    /// The thread member objects for each returned thread the current user has joined.
    /// </summary>
    public IReadOnlyList<IThreadMember> Members { get; }

    /// <summary>
    /// Indicates whether there are potentially additional threads that could be returned on a subsequent call.
    /// </summary>
    public bool HasMore { get; }
}
