// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IReaction" />
public sealed record Reaction : IReaction
{
    /// <inheritdoc/>
    public required int Count { get; init; }

    /// <inheritdoc/>
    public required IReactionCountDetails CountDetails { get; init; }

    /// <inheritdoc/>
    public required bool Me { get; init; }

    /// <inheritdoc/>
    public required bool MeBurst { get; init; }

    /// <inheritdoc/>
    public required IPartialEmoji Emoji { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<int> BurstColors { get; init; }
}