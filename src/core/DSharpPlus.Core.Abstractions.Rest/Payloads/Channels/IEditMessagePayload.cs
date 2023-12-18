// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /channels/:channel-id/messages/:message-id</c>.
/// </summary>
public interface IEditMessagePayload
{
    /// <summary>
    /// New string content of the message, up to 2000 characters.
    /// </summary>
    public Optional<string?> Content { get; }

    /// <summary>
    /// Up to 10 embeds for this message.
    /// </summary>
    public Optional<IReadOnlyList<IEmbed>?> Embeds { get; }

    /// <summary>
    /// New flags for this message. Only <see cref="DiscordMessageFlags.SuppressEmbeds"/> can currently be set 
    /// or unset.
    /// </summary>
    public Optional<DiscordMessageFlags?> Flags { get; }

    /// <summary>
    /// Authoritative allowed mentions object for this message. Passing <see langword="null"/> <b>resets</b> 
    /// the object to default.
    /// </summary>
    public Optional<IAllowedMentions?> AllowedMentions { get; }

    /// <summary>
    /// New components for this message.
    /// </summary>
    public Optional<IReadOnlyList<IActionRowComponent>?> Components { get; }

    /// <summary>
    /// Attached files to this message. This must include old attachments to be retained and new attachments, 
    /// if passed.
    /// </summary>
    public IReadOnlyList<ImageData>? Files { get; }

    /// <summary>
    /// Attachments to this message.
    /// </summary>
    public Optional<IEnumerable<IPartialAttachment>?> Attachments { get; }
}
