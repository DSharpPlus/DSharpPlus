// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IReactionCountDetails" />
public sealed record ReactionCountDetails : IReactionCountDetails
{
    /// <inheritdoc/>
    public required int Burst { get; init; }

    /// <inheritdoc/>
    public required int Normal { get; init; }
}