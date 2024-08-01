// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Provides additional information about how many times a reaction was applied to a message.
/// </summary>
public interface IReactionCountDetails
{
    /// <summary>
    /// Specifies the amount of super reactions of this emoji applied to the message.
    /// </summary>
    public int Burst { get; }

    /// <summary>
    /// Specifies the amount of standard reactions of this emoji applied to the message.
    /// </summary>
    public int Normal { get; }
}
