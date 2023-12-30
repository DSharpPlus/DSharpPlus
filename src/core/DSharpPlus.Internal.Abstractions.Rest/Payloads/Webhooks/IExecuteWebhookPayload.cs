// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /webhooks/:webhook-id/:webhook-token</c>.
/// </summary>
public interface IExecuteWebhookPayload
{
    /// <summary>
    /// Message contents.
    /// </summary>
    public Optional<string> Content { get; }

    /// <summary>
    /// Overrides the default username of the webhook.
    /// </summary>
    public Optional<string> Username { get; }

    /// <summary>
    /// Overrides the default avatar of the webhook.
    /// </summary>
    public Optional<string> AvatarUrl { get; }

    /// <summary>
    /// True if this is a TTS message.
    /// </summary>
    public Optional<bool> Tts { get; }

    /// <summary>
    /// Up to 10 embeds
    /// </summary>
    public Optional<IReadOnlyList<IEmbed>> Embeds { get; }

    /// <summary>
    /// Allowed mentions object for this message.
    /// </summary>
    public Optional<IAllowedMentions> AllowedMentions { get; }

    /// <summary>
    /// Components to include with this message.
    /// </summary>
    public Optional<IReadOnlyList<IActionRowComponent>> Components { get; }

    /// <summary>
    /// Attachment files to include with this message.
    /// </summary>
    public IReadOnlyList<AttachmentData>? Files { get; }

    /// <summary>
    /// Attachment descriptor objects with filename and description.
    /// </summary>
    public Optional<IReadOnlyList<IPartialAttachment>> Attachments { get; }

    /// <summary>
    /// Message flags for this message. Only <see cref="DiscordMessageFlags.SuppressEmbeds"/> can be set.
    /// </summary>
    public Optional<DiscordMessageFlags> Flags { get; }

    /// <summary>
    /// The name of the thread to create, if the webhook channel is a forum channel.
    /// </summary>
    public Optional<string> ThreadName { get; }

    /// <summary>
    /// The snowflake identifiers of tags to apply to the thread, if applicable.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> AppliedTags { get; }
}
