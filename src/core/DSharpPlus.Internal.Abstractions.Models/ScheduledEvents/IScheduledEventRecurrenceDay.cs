// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Describes which day within a month an event should recur on.
/// </summary>
public interface IScheduledEventRecurrenceDay
{
    /// <summary>
    /// The week of the month this recurrence point refers to. Restricted to 1-5.
    /// </summary>
    public int N { get; }

    /// <summary>
    /// The day of the specified week to recur at.
    /// </summary>
    public DiscordScheduledEventRecurrenceWeekday Day { get; }
}
