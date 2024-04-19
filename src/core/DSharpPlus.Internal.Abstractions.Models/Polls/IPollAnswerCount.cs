// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents the answer count for a poll.
/// </summary>
public interface IPollAnswerCount
{
    /// <summary>
    /// The ID of the answer, corresponds to <seealso cref="IPollAnswer.AnswerId"/>.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The amount of votes this answer received.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Indicates whether the current user has voted on this poll. Applications are not allowed to vote on polls.
    /// </summary>
    public bool MeVoted { get; }
}
