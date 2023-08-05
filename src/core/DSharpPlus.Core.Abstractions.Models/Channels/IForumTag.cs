// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a single forum tag, applicable to forum or media channel posts.
/// </summary>
public interface IForumTag
{
    /// <summary>
    /// The snowflake identifier of this tag.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The name of this tag, up to 20 characters.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Indicates whether this tag can only be added or removed by a member with the manage threads permission.
    /// </summary>
    public bool Moderated { get; }

    /// <summary>
    /// The snowflake identifier of a custom emoji to be applied to this tag. Mutually exclusive with
    /// <seealso cref="EmojiName"/>.
    /// </summary>
    public Snowflake? EmojiId { get; }

    /// <summary>
    /// The unicode representation of a default emoji to be applied to this tag. Mutually exclusive with
    /// <seealso cref="EmojiId"/>.
    /// </summary>
    public string? EmojiName { get; }
}
