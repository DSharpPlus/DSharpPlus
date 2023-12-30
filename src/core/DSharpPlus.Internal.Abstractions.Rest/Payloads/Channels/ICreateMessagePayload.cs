// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to POST /channels/:channel-id/messages.
/// </summary>
public interface ICreateMessagePayload
{
    /// <summary>
    /// The message content to send.
    /// </summary>
    public Optional<string> Content { get; }

    /// <summary>
    /// An identifier for this message. This will be sent in the MESSAGE_CREATE event.
    /// </summary>
    public Optional<string> Nonce { get; }

    /// <summary>
    /// Indicates whether this message is a text-to-speech message.
    /// </summary>
    public Optional<bool> Tts { get; }

    /// <summary>
    /// Up to 10 embeds to be attached to this message.
    /// </summary>
    public Optional<IReadOnlyList<IEmbed>> Embeds { get; }

    /// <summary>
    /// Specifies which mentions should be resolved.
    /// </summary>
    public Optional<IAllowedMentions> AllowedMentions { get; }

    /// <summary>
    /// A reference to the message this message shall reply to.
    /// </summary>
    public Optional<IMessageReference> MessageReference { get; }

    /// <summary>
    /// A list of components to include with the message.
    /// </summary>
    public Optional<IReadOnlyList<IActionRowComponent>> Components { get; }

    /// <summary>
    /// Up to 3 snowflake identifiers of stickers to be attached to this message.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> StickerIds { get; }

    /// <summary>
    /// Files to be attached to this message.
    /// </summary>
    public IReadOnlyList<AttachmentData>? Files { get; }

    /// <summary>
    /// Attachment metadata for this message.
    /// </summary>
    public Optional<IReadOnlyList<IPartialAttachment>> Attachments { get; }

    /// <summary>
    /// Message flags, combined as bitfield. Only <see cref="DiscordMessageFlags.SuppressEmbeds"/> can be set.
    /// </summary>
    public Optional<DiscordMessageFlags> Flags { get; }
}
