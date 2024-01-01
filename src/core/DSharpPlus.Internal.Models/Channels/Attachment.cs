// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IAttachment" />
public sealed record Attachment : IAttachment
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required string Filename { get; init; }

    /// <inheritdoc/>
    public Optional<string> Description { get; init; }

    /// <inheritdoc/>
    public Optional<string> ContentType { get; init; }

    /// <inheritdoc/>
    public required int Size { get; init; }

    /// <inheritdoc/>
    public required string Url { get; init; }

    /// <inheritdoc/>
    public required string ProxyUrl { get; init; }

    /// <inheritdoc/>
    public Optional<int?> Height { get; init; }

    /// <inheritdoc/>
    public Optional<int?> Width { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Ephemeral { get; init; }

    /// <inheritdoc/>
    public Optional<float> DurationSecs { get; init; }

    /// <inheritdoc/>
    public Optional<ReadOnlyMemory<byte>> Waveform { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordAttachmentFlags> Flags { get; init; }
}