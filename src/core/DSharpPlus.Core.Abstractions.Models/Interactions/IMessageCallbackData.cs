// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a response payload for creating or updating a message.
/// </summary>
public interface IMessageCallbackData
{
    /// <summary>
    /// Indicates whether the response is a TTS message.
    /// </summary>
    public Optional<bool> Tts { get; }

    /// <summary>
    /// The message content.
    /// </summary>
    public Optional<string> Content { get; }

    /// <summary>
    /// An array of up to 10 embeds to be attached to the message.
    /// </summary>
    public Optional<IReadOnlyList<IEmbed>> Embeds { get; }

    /// <summary>
    /// An allowed mentions object controlling mention behaviour for this method.
    /// </summary>
    public Optional<IAllowedMentions> AllowedMentions { get; }

    /// <summary>
    /// Message flags for this message; only SuppressEmbeds and Ephemeral can be set.
    /// </summary>
    public Optional<DiscordMessageFlags> Flags { get; }

    /// <summary>
    /// Up to five action rows of components to attach to this message.
    /// </summary>
    public Optional<IReadOnlyList<IActionRowComponent>> Components { get; }

    /// <summary>
    /// Attachments to this message.
    /// </summary>
    public Optional<IReadOnlyList<IPartialAttachment>> Attachments { get; }
}
