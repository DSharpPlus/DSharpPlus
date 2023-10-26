// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IReaction" />
public sealed record Reaction : IReaction
{
    /// <inheritdoc/>
    public required int Count { get; init; }

    /// <inheritdoc/>
    public required bool Me { get; init; }

    /// <inheritdoc/>
    public required IPartialEmoji Emoji { get; init; }
}