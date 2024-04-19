// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains the ThreadId query parameter for webhook requests that can target a message in a thread.
/// </summary>
public readonly record struct ThreadIdQuery
{
    /// <summary>
    /// Specifies a thread to search in rather than directly to the parent channel. If the thread is archived,
    /// and this is an edit, this will automatically unarchive it. Only threads with the same parent channel
    /// as the webhook can be passed.
    /// </summary>
    public Snowflake? ThreadId { get; init; }
}
