// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IIncidentsData" />
public sealed record IncidentsData : IIncidentsData
{
    /// <inheritdoc/>
    public DateTimeOffset? InvitesDisabledUntil { get; init; }

    /// <inheritdoc/>
    public DateTimeOffset? DmsDisabledUntil { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset?> DmSpamDetectedAt { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset?> RaidDetectedAt { get; init; }
}