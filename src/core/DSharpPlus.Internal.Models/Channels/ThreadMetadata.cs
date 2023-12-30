// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IThreadMetadata" />
public sealed record ThreadMetadata : IThreadMetadata
{
    /// <inheritdoc/>
    public required bool Archived { get; init; }

    /// <inheritdoc/>
    public required int AutoArchiveDuration { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset ArchiveTimestamp { get; init; }

    /// <inheritdoc/>
    public required bool Locked { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Invitable { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset?> CreateTimestamp { get; init; }
}