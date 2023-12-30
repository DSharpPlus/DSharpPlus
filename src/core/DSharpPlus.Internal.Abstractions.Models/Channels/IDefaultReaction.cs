// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Indicates the default emoji to react to a forum post with.
/// </summary>
public interface IDefaultReaction
{
    /// <summary>
    /// The snowflake identifier of a custom emoji to react with. Mutually exclusive with
    /// <seealso cref="EmojiName"/>.
    /// </summary>
    public Snowflake? EmojiId { get; }

    /// <summary>
    /// The unicode representation of a default emoji to react with. Mutually exclusive with
    /// <seealso cref="EmojiId"/>.
    /// </summary>
    public string? EmojiName { get; }
}
