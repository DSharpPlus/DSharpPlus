// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IPollAnswer" />
public sealed record PollAnswer : IPollAnswer
{
    /// <inheritdoc/>
    public required int AnswerId { get; init; }

    /// <inheritdoc/>
    public required IPollMedia PollMedia { get; init; }
}
