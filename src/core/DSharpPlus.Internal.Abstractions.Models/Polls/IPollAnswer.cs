// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents an answer option to a poll.
/// </summary>
public interface IPollAnswer
{
    /// <summary>
    /// The numeric ID of this answer.
    /// </summary>
    /// <remarks>
    /// This is only sent as part of responses from the API or gateway.
    /// </remarks>
    public int AnswerId { get; }

    /// <summary>
    /// The text and optional emoji of this answer. The text is limited to 55 characters.
    /// </summary>
    public IPollMedia PollMedia { get; }
}
