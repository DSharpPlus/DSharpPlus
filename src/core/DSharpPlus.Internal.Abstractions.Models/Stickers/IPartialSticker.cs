// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a partially populated sticker object.
/// </summary>
public interface IPartialSticker
{
    /// <summary>
    /// The snowflake identifier of this sticker.
    /// </summary>
    public Optional<Snowflake> Id { get; }

    /// <summary>
    /// For standard stickers, the snowflake identifier of the pack the sticker is from.
    /// </summary>
    public Optional<Snowflake> PackId { get; }

    /// <summary>
    /// The name of this sticker.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// The description of this sticker.
    /// </summary>
    public Optional<string?> Description { get; }

    /// <summary>
    /// Autocomplete/suggestion tags for this sticker, up to 200 characters.
    /// </summary>
    /// <remarks>
    /// For standard stickers, this is a comma separated list of keywords. When creating or modifying a guild
    /// sticker, the client will always use a name generated from an emoji here.
    /// </remarks>
    public Optional<string> Tags { get; }

    /// <summary>
    /// The type of this sticker.
    /// </summary>
    public Optional<DiscordStickerType> Type { get; }

    /// <summary>
    /// The type of this sticker file format.
    /// </summary>
    public Optional<DiscordStickerFormatType> FormatType { get; }

    /// <summary>
    /// Indicates whether this sticker can be used.
    /// </summary>
    public Optional<bool> Available { get; }

    /// <summary>
    /// The snowflake identifier of the guild that owns this object.
    /// </summary>
    public Optional<Snowflake> GuildId { get; }

    /// <summary>
    /// The user that uploaded this sticker.
    /// </summary>
    public Optional<IUser> User { get; }

    /// <summary>
    /// If this is a standard sticker, the sort order within its pack.
    /// </summary>
    public Optional<int> SortValue { get; }
}
