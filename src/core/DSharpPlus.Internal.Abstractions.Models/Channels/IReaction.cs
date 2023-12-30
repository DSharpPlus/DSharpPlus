// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a reaction to a message.
/// </summary>
public interface IReaction
{
    /// <summary>
    /// The amount of times this emoji has been reacted with.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Indicates whether the current user has reacted using this emoji.
    /// </summary>
    public bool Me { get; }

    /// <summary>
    /// The emoji that is being reacted with.
    /// </summary>
    public IPartialEmoji Emoji { get; }
}
