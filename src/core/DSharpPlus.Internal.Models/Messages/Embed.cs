// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IEmbed" />
public sealed record Embed : IEmbed
{
    /// <inheritdoc/>
    public Optional<string> Title { get; init; }

    /// <inheritdoc/>
    public Optional<string> Type { get; init; }

    /// <inheritdoc/>
    public Optional<string> Description { get; init; }

    /// <inheritdoc/>
    public Optional<string> Url { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset> Timestamp { get; init; }

    /// <inheritdoc/>
    public Optional<int> Color { get; init; }

    /// <inheritdoc/>
    public Optional<IEmbedFooter> Footer { get; init; }

    /// <inheritdoc/>
    public Optional<IEmbedImage> Image { get; init; }

    /// <inheritdoc/>
    public Optional<IEmbedThumbnail> Thumbnail { get; init; }

    /// <inheritdoc/>
    public Optional<IEmbedVideo> Video { get; init; }

    /// <inheritdoc/>
    public Optional<IEmbedProvider> Provider { get; init; }

    /// <inheritdoc/>
    public Optional<IEmbedAuthor> Author { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IEmbedField>> Fields { get; init; }
}