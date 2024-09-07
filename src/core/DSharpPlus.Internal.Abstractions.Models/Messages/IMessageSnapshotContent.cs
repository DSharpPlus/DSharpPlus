// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;
using System.Collections.Generic;
using System;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents the contents of a <see cref="IMessageSnapshot"/>.
/// </summary>
public interface IMessageSnapshotContent
{
    /// <summary>
    /// The text contents of this message. This will be empty if your application does not have the
    /// message content intent.
    /// </summary>
    public Optional<string> Content { get; }

    /// <summary>
    /// The timestamp at which this message was sent.
    /// </summary>
    public Optional<DateTimeOffset> Timestamp { get; }

    /// <summary>
    /// The timestamp at which this message was last edited.
    /// </summary>
    public Optional<DateTimeOffset?> EditedTimestamp { get; }

    /// <summary>
    /// The users specifically mentioned in this message.
    /// </summary>
    public Optional<IReadOnlyList<IUser>> Mentions { get; }

    /// <summary>
    /// The roles specifically mentioned in this message.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> MentionRoles { get; }

    /// <summary>
    /// The files attached to this message.
    /// </summary>
    public Optional<IReadOnlyList<IAttachment>> Attachments { get; }

    /// <summary>
    /// The embeds added to this message.
    /// </summary>
    public Optional<IReadOnlyList<IEmbed>> Embeds { get; }

    /// <summary>
    /// The type of this message.
    /// </summary>
    public Optional<DiscordMessageType> Type { get; }

    /// <summary>
    /// Additional flags for this message.
    /// </summary>
    public Optional<DiscordMessageFlags> Flags { get; }

    /// <summary>
    /// The components attached to this message.
    /// </summary>
    public Optional<IReadOnlyList<IActionRowComponent>> Components { get; }

    /// <summary>
    /// The stickers sent along with this message.
    /// </summary>
    public Optional<IStickerItem> StickerItems { get; }
}
