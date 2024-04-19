// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IPollMedia" />
public sealed record PollMedia : IPollMedia
{
    /// <inheritdoc/>
    public Optional<string> Text { get; init; }

    /// <inheritdoc/>
    public Optional<IPartialEmoji> Emoji { get; init; }
}
