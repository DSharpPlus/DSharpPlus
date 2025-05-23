// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IUnfurledMediaItem" />
public sealed record UnfurledMediaItem : IUnfurledMediaItem
{
    /// <inheritdoc/>
    public required string Url { get; init; }

    /// <inheritdoc/>
    public Optional<string> ProxyUrl { get; init; }

    /// <inheritdoc/>
    public Optional<int?> Height { get; init; }

    /// <inheritdoc/>
    public Optional<int?> Width { get; init; }

    /// <inheritdoc/>
    public Optional<string> ContentType { get; init; }
}