// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IMessageCall" />
public sealed record MessageCall : IMessageCall
{
    /// <inheritdoc/>
    public required IReadOnlyList<Snowflake> Participants { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset?> EndedTimestamp { get; init; }
}