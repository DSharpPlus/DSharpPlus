// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /webhooks/:application-id/:interaction-token/messages/:message-id</c>.
/// </summary>
public interface IEditFollowupMessagePayload
{
    /// <summary>
    /// The message contents, up to 2000 characters.
    /// </summary>
    public Optional<string?> Content { get; }

    /// <summary>
    /// Up to 10 embeds, subject to embed length limits.
    /// </summary>
    public Optional<IReadOnlyList<IEmbed>?> Embeds { get; }

    /// <summary>
    /// Allowed mentions for this message.
    /// </summary>
    public Optional<IAllowedMentions?> AllowedMentions { get; }

    /// <summary>
    /// The components for this message.
    /// </summary>
    public Optional<IReadOnlyList<IComponent>?> Components { get; }

    /// <summary>
    /// Attached files to keep and possible descriptions for new files to upload.
    /// </summary>
    public Optional<IReadOnlyList<IPartialAttachment>?> Attachments { get; }

    /// <summary>
    /// File contents to send or edit.
    /// </summary>
    public IReadOnlyList<AttachmentData>? Files { get; }
}
