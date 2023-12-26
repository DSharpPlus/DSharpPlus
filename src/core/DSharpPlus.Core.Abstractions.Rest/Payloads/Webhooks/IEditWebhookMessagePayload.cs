// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /webhooks/:webhook-id/:webhook-token/messages/:message-id</c>.
/// </summary>
public interface IEditWebhookMessagePayload
{
    /// <summary>
    /// The new message contents.
    /// </summary>
    public Optional<string?> Content { get; }

    /// <summary>
    /// Up to 10 embeds attached to the message.
    /// </summary>
    public Optional<IReadOnlyList<IEmbed>?> Embeds { get; }

    /// <summary>
    /// The new allowed mentions object for this message.
    /// </summary>
    public Optional<IAllowedMentions?> AllowedMentions { get; }

    /// <summary>
    /// The new components attached to this message.
    /// </summary>
    public Optional<IReadOnlyList<IActionRowComponent>?> Components { get; }

    /// <summary>
    /// The new files to be sent along with the edit. Note that all used files in this message must be passed here,
    /// even if they were originally present.
    /// </summary>
    public IReadOnlyList<AttachmentData>? Files { get; }

    /// <summary>
    /// Attachment descriptor objects for this message.
    /// </summary>
    public Optional<IReadOnlyList<IPartialAttachment>?> Attachments { get; }
}
