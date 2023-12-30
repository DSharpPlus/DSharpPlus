// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IGuildPreview" />
public sealed record GuildPreview : IGuildPreview
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public string? Icon { get; init; }

    /// <inheritdoc/>
    public string? Splash { get; init; }

    /// <inheritdoc/>
    public string? DiscoverySplash { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IEmoji> Emojis { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<string> Features { get; init; }

    /// <inheritdoc/>
    public required int ApproximateMemberCount { get; init; }

    /// <inheritdoc/>
    public required int ApproximatePresenceCount { get; init; }

    /// <inheritdoc/>
    public string? Description { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<ISticker> Stickers { get; init; }
}