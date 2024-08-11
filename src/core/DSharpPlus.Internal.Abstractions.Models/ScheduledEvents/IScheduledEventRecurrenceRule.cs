// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a rule for recurring scheduled events. This is a subset of the 
/// <see href="https://datatracker.ietf.org/doc/html/rfc5545">iCalendar specification</see> and has some rather specific
/// limitations, documented where appropriate.
/// </summary>
public interface IScheduledEventRecurrenceRule
{
    /// <summary>
    /// Specifies the starting time of the recurrence interval.
    /// </summary>
    public DateTimeOffset Start { get; }

    /// <summary>
    /// Specifies the ending time of the recurrence interval. This cannot be set by the application.
    /// </summary>
    public DateTimeOffset? End { get; }

    /// <summary>
    /// Specifies how often this event should occur.
    /// </summary>
    public DiscordScheduledEventRecurrenceFrequency Frequency { get; }

    /// <summary>
    /// Specifies the interval according to <see cref="Frequency"/>. For example, a frequency of
    /// <see cref="DiscordScheduledEventRecurrenceFrequency.Weekly"/> with an interval of 2 would mean "every other week".
    /// </summary>
    /// <remarks>
    /// This can only be set to 1 or 2 (not higher) for weekly events, and exactly 1 for all other events.
    /// </remarks>
    public int Interval { get; }

    /// <summary>
    /// Specifies a set of specific days within a week to recur on. This is mutually exclusive with
    /// <see cref="ByNWeekday"/> and <see cref="ByMonth"/> + <see cref="ByMonthDay"/>.
    /// </summary>
    /// <remarks>
    /// This field is only valid for daily and weekly events according to <see cref="Frequency"/>. However, it also
    /// behaves differently depending on the frequency: <br/><br/>
    /// IF the frequency is set to <see cref="DiscordScheduledEventRecurrenceFrequency.Daily"/>, the values must be a
    /// "known set" of weekdays. The following sets are currently allowed: <br/>
    /// - Monday to Friday <br/>
    /// - Tuesday to Saturday <br/>
    /// - Sunday to Thursday <br/>
    /// - Friday and Saturday <br/>
    /// - Saturday and Sunday <br/>
    /// - Sunday and Monday <br/> <br/>
    /// IF the frequency is set to <see cref="DiscordScheduledEventRecurrenceFrequency.Weekly"/>, the array must have
    /// a length of 1, so only one weekday can have a recurring event on. If you wish to have an event recur on multiple
    /// days within a week, use a daily-frequency event.
    /// </remarks>
    public IReadOnlyList<DiscordScheduledEventRecurrenceWeekday>? ByWeekday { get; }

    /// <summary>
    /// Specifies a set of specific days within a month to recur on. This is mutually exclusive with
    /// <see cref="ByWeekday"/> and <see cref="ByMonth"/> + <see cref="ByMonthDay"/>
    /// </summary>
    /// <remarks>
    /// This field is only valid for monthly events according to <see cref="Frequency"/>. It may only contain
    /// a single element.
    /// </remarks>
    public IReadOnlyList<IScheduledEventRecurrenceDay>? ByNWeekday { get; }

    /// <summary>
    /// Specifies a set of months within a year to recur in. This is mutually exclusive with <see cref="ByWeekday"/>
    /// and <see cref="ByNWeekday"/> and requires <see cref="ByMonthDay"/> to also be specified.
    /// </summary>
    /// <remarks>
    /// This field is only valid for yearly events according to <see cref="Frequency"/>. It may only contain a single
    /// element.
    /// </remarks>
    public IReadOnlyList<DiscordScheduledEventRecurrenceMonth>? ByMonth { get; }

    /// <summary>
    /// Specifies a date within a month to recur on. This is mutually exclusive with <see cref="ByWeekday"/> and
    /// <see cref="ByNWeekday"/> and requires <see cref="ByMonth"/> to also be specified.
    /// </summary>
    /// <remarks>
    /// This field is only valid for yearly events according to <see cref="Frequency"/>. It may only contain a single
    /// element.
    /// </remarks>
    public IReadOnlyList<int>? ByMonthDay { get; }

    /// <summary>
    /// Specifies the set of days within the year to recur on. This cannot be set by the application.
    /// </summary>
    public IReadOnlyList<int>? ByYearDay { get; }

    /// <summary>
    /// Specifies the total amount of times this event is allowed to recur before stopping. This cannot be set by the
    /// application.
    /// </summary>
    public int? Count { get; }
}
