// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /webhooks/:application-id/:interaction-token</c>.
/// </summary>
/// <remarks>
/// Either <seealso cref="Content"/>, <seealso cref="Embeds"/> or <seealso cref="Attachments"/> must be set.
/// </remarks>
public interface ICreateFollowupMessagePayload
{
    /// <summary>
    /// The text contents for this message, up to 2000 characters.
    /// </summary>
    public Optional<string> Content { get; }

    /// <summary>
    /// Indicates whether this is a TTS message.
    /// </summary>
    public Optional<bool> Tts { get; }

    /// <summary>
    /// Embeds attached to this message.
    /// </summary>
    public Optional<IReadOnlyList<IEmbed>> Embeds { get; }

    /// <summary>
    /// An allowed mentions object for this message.
    /// </summary>
    public Optional<IAllowedMentions> AllowedMentions { get; }

    /// <summary>
    /// Up to five action rows worth of components to include with this message.
    /// </summary>
    public Optional<IReadOnlyList<IActionRowComponent>> Components { get; }

    /// <summary>
    /// Files to upload with this message.
    /// </summary>
    public Optional<IReadOnlyList<AttachmentData>> Files { get; }

    /// <summary>
    /// Attachment metadata for files uploaded with this message.
    /// </summary>
    public Optional<IReadOnlyList<IPartialAttachment>> Attachments { get; }

    /// <summary>
    /// Additional message flags for this message. SuppressEmbeds and Ephemeral can be set.
    /// </summary>
    public Optional<DiscordMessageFlags> Flags { get; }
}
