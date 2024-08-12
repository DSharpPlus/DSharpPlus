// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Extensions.Internal.Toolbox.Implementations;

/// <inheritdoc cref="IScheduledEventRecurrenceDay" />
internal sealed record BuiltScheduledEventRecurrenceDay : IScheduledEventRecurrenceDay
{
    /// <inheritdoc/>
    public required int N { get; init; }

    /// <inheritdoc/>
    public required DiscordScheduledEventRecurrenceWeekday Day { get; init; }
}