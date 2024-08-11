// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Extensions.Internal.Toolbox.Implementations;

/// <inheritdoc cref="IScheduledEventRecurrenceRule" />
internal sealed record BuiltScheduledEventRecurrenceRule : IScheduledEventRecurrenceRule
{
    /// <inheritdoc/>
    public required DateTimeOffset Start { get; init; }

    /// <inheritdoc/>
    public DateTimeOffset? End { get; init; }

    /// <inheritdoc/>
    public required DiscordScheduledEventRecurrenceFrequency Frequency { get; init; }

    /// <inheritdoc/>
    public required int Interval { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<DiscordScheduledEventRecurrenceWeekday>? ByWeekday { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<IScheduledEventRecurrenceDay>? ByNWeekday { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<DiscordScheduledEventRecurrenceMonth>? ByMonth { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<int>? ByMonthDay { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<int>? ByYearDay { get; init; }

    /// <inheritdoc/>
    public int? Count { get; init; }
}