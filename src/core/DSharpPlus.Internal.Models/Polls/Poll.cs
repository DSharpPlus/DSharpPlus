// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IPoll" />
public sealed record Poll : IPoll
{
    /// <inheritdoc/>
    public required IPollMedia Question { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IPollAnswer> Answers { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset Expiry { get; init; }

    /// <inheritdoc/>
    public required bool AllowMultiselect { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordPollLayoutType> LayoutType { get; init; }
}
