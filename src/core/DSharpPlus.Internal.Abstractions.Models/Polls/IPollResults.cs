// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents the results from a poll.
/// </summary>
/// <remarks>
/// While a poll is in progress, the results may not be entirely accurate, though they shouldn't deviate by much.
/// After a poll finishes, Discord performs a final, accurate tally. If <seealso cref="IsFinalized"/> is set to true,
/// this tally has concluded and the results are accurate.
/// </remarks>
public interface IPollResults
{
    /// <summary>
    /// Indicates whether the votes have been fully counted.
    /// </summary>
    public bool IsFinalized { get; }

    /// <summary>
    /// The counts for each answer. If an answer is missing, it received 0 votes.
    /// </summary>
    public IReadOnlyList<IPollAnswerCount> AnswerCounts { get; }
}
