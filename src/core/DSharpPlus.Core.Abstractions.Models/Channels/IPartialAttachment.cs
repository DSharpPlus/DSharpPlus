// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a partially populated message attachment.
/// </summary>
public interface IPartialAttachment
{
    /// <summary>
    /// The snowflake identifier of this attachment.
    /// </summary>
    public Optional<Snowflake> Id { get; }

    /// <summary>
    /// The attachment's filename.
    /// </summary>
    public Optional<string> Filename { get; }

    /// <summary>
    /// The file description, up to 1024 characters.
    /// </summary>
    public Optional<string> Description { get; }

    /// <summary>
    /// This attachment's media type.
    /// </summary>
    public Optional<string> ContentType { get; }

    /// <summary>
    /// The file size in bytes.
    /// </summary>
    public Optional<int> Size { get; }

    /// <summary>
    /// The source URL of this file.
    /// </summary>
    public Optional<string> Url { get; }

    /// <summary>
    /// A proxied URL of this file.
    /// </summary>
    public Optional<string> ProxyUrl { get; }

    /// <summary>
    /// The height of this file, if this is an image.
    /// </summary>
    public Optional<int?> Height { get; }

    /// <summary>
    /// The width of this file, if this is an image.
    /// </summary>
    public Optional<int?> Width { get; }

    /// <summary>
    /// Indicates whether this is an ephemeral attachment.
    /// </summary>
    public Optional<bool> Ephemeral { get; }

    /// <summary>
    /// The duration of this voice message in seconds.
    /// </summary>
    public Optional<float> DurationSecs { get; }

    /// <summary>
    /// base64-encoded byte array representing a sampled waveform for this voice message.
    /// </summary>
    public Optional<ReadOnlyMemory<byte>> Waveform { get; }

    /// <summary>
    /// Additional flags for this attachment.
    /// </summary>
    public Optional<DiscordAttachmentFlags> Flags { get; }
}
