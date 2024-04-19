// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

public interface IForumAndMediaThreadMessage
{
    /// <summary>
    /// The message content to send.
    /// </summary>
    public Optional<string> Content { get; }

    /// <summary>
    /// Up to 10 embeds to be attached to this message.
    /// </summary>
    public Optional<IReadOnlyList<IEmbed>> Embeds { get; }

    /// <summary>
    /// Specifies which mentions should be resolved.
    /// </summary>
    public Optional<IAllowedMentions> AllowedMentions { get; }

    /// <summary>
    /// A list of components to include with the message.
    /// </summary>
    public Optional<IReadOnlyList<IActionRowComponent>> Components { get; }

    /// <summary>
    /// Up to 3 snowflake identifiers of stickers to be attached to this message.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> StickerIds { get; }

    /// <summary>
    /// Attachment metadata for this message.
    /// </summary>
    /// <remarks>
    /// The files have to be attached to the parent object.
    /// </remarks>
    public Optional<IReadOnlyList<IPartialAttachment>> Attachments { get; }

    /// <summary>
    /// Message flags, combined as bitfield. Only <see cref="DiscordMessageFlags.SuppressEmbeds"/> can be set.
    /// </summary>
    public Optional<DiscordMessageFlags> Flags { get; }
}
