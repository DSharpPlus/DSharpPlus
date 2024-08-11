// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Entities;
using DSharpPlus.Extensions.Internal.Toolbox.Implementations;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Extensions.Internal.Toolbox;

/// <summary>
/// Provides utilities for writing recurrence rules.
/// </summary>
public static class RecurrenceRule
{
    /// <inheritdoc cref="Yearly(DateOnly, DateTimeOffset)"/>
    public static IScheduledEventRecurrenceRule Yearly(DateOnly date)
        => Yearly(date, DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a rule to recur every year on the specified date.
    /// </summary>
    /// <param name="date">The date to recur on. <see cref="DateOnly.Year"/> will be ignored.</param>
    /// <param name="startDate">The starting date of the recurring event.</param>
    public static IScheduledEventRecurrenceRule Yearly(DateOnly date, DateTimeOffset startDate)
    {
        return new BuiltScheduledEventRecurrenceRule
        {
            Start = startDate,
            Frequency = DiscordScheduledEventRecurrenceFrequency.Yearly,
            ByMonth = [(DiscordScheduledEventRecurrenceMonth)date.Month],
            ByMonthDay = [date.Day],
            Interval = 1
        };
    }

    /// <inheritdoc cref="Monthly(int, DayOfWeek, DateTimeOffset)"/>
    public static IScheduledEventRecurrenceRule Monthly(int week, DayOfWeek day)
        => Monthly(week, day, DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a rule to recur every month on the specified day.
    /// </summary>
    /// <param name="week">Specifies the week within the month to recur in.</param>
    /// <param name="day">Specifies the day within the week to recur on.</param>
    /// <param name="startDate">The starting date of the recurring event.</param>
    public static IScheduledEventRecurrenceRule Monthly(int week, DayOfWeek day, DateTimeOffset startDate)
    {
        return new BuiltScheduledEventRecurrenceRule
        {
            Start = startDate,
            Frequency = DiscordScheduledEventRecurrenceFrequency.Monthly,
            ByNWeekday =
            [
                new BuiltScheduledEventRecurrenceDay
                {
                    Day = ToMondayWeek(day),
                    N = week
                }
            ],
            Interval = 1
        };
    }

    /// <inheritdoc cref="Weekly(DayOfWeek, DateTimeOffset)"/>
    public static IScheduledEventRecurrenceRule Weekly(DayOfWeek day)
        => Weekly(day, DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a new recurrence rule to recur every week on the specified weekday.
    /// </summary>
    /// <param name="day">The weekday to recur on.</param>
    /// <param name="startDate">The starting date of the recurring event.</param>
    public static IScheduledEventRecurrenceRule Weekly(DayOfWeek day, DateTimeOffset startDate)
    {
        return new BuiltScheduledEventRecurrenceRule
        {
            Start = startDate,
            Frequency = DiscordScheduledEventRecurrenceFrequency.Weekly,
            ByWeekday = [ToMondayWeek(day)],
            Interval = 1
        };
    }

    /// <inheritdoc cref="BiWeekly(DayOfWeek, DateTimeOffset)"/>
    public static IScheduledEventRecurrenceRule BiWeekly(DayOfWeek day)
        => BiWeekly(day, DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a new recurrence rule to recur every other week on the specified weekday.
    /// </summary>
    /// <param name="day">The weekday to recur on.</param>
    /// <param name="startDate">The starting date of the recurring event.</param>
    public static IScheduledEventRecurrenceRule BiWeekly(DayOfWeek day, DateTimeOffset startDate)
    {
        return new BuiltScheduledEventRecurrenceRule
        {
            Start = startDate,
            Frequency = DiscordScheduledEventRecurrenceFrequency.Weekly,
            ByWeekday = [ToMondayWeek(day)],
            Interval = 2
        };
    }

    /// <inheritdoc cref="Daily(DateTimeOffset)"/>
    public static IScheduledEventRecurrenceRule Daily()
        => Daily(DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a new recurrence rule to recur every day.
    /// </summary>
    /// <param name="startDate">The starting date of the recurring event.</param>
    public static IScheduledEventRecurrenceRule Daily(DateTimeOffset startDate)
    {
        return new BuiltScheduledEventRecurrenceRule
        {
            Start = startDate,
            Frequency = DiscordScheduledEventRecurrenceFrequency.Daily,
            Interval = 1
        };
    }

    /// <inheritdoc cref="OnWorkingDays(DateTimeOffset)"/>
    public static IScheduledEventRecurrenceRule OnWorkingDays()
        => OnWorkingDays(DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a new recurrence rule to recur every working day, monday to friday.
    /// </summary>
    /// <param name="startDate">The starting date of the recurring event.</param>
    public static IScheduledEventRecurrenceRule OnWorkingDays(DateTimeOffset startDate)
    {
        return new BuiltScheduledEventRecurrenceRule
        {
            Start = startDate,
            Frequency = DiscordScheduledEventRecurrenceFrequency.Daily,
            ByWeekday =
            [
                DiscordScheduledEventRecurrenceWeekday.Monday,
                DiscordScheduledEventRecurrenceWeekday.Tuesday,
                DiscordScheduledEventRecurrenceWeekday.Wednesday,
                DiscordScheduledEventRecurrenceWeekday.Thursday,
                DiscordScheduledEventRecurrenceWeekday.Friday
            ],
            Interval = 1
        };
    }

    /// <inheritdoc cref="OnWeekends(DateTimeOffset)"/>
    public static IScheduledEventRecurrenceRule OnWeekends()
        => OnWeekends(DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates a new recurrence rule to recur every weekend, on saturday and sunday.
    /// </summary>
    /// <param name="startDate">The starting date of the recurring event.</param>
    public static IScheduledEventRecurrenceRule OnWeekends(DateTimeOffset startDate)
    {
        return new BuiltScheduledEventRecurrenceRule
        {
            Start = startDate,
            Frequency = DiscordScheduledEventRecurrenceFrequency.Daily,
            ByWeekday =
            [
                DiscordScheduledEventRecurrenceWeekday.Saturday,
                DiscordScheduledEventRecurrenceWeekday.Sunday
            ],
            Interval = 1
        };
    }

    // the enum layout of DayOfWeek and DiscordScheduledEventRecurrenceWeekday isn't the same.
    private static DiscordScheduledEventRecurrenceWeekday ToMondayWeek(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => DiscordScheduledEventRecurrenceWeekday.Monday,
            DayOfWeek.Tuesday => DiscordScheduledEventRecurrenceWeekday.Tuesday,
            DayOfWeek.Wednesday => DiscordScheduledEventRecurrenceWeekday.Wednesday,
            DayOfWeek.Thursday => DiscordScheduledEventRecurrenceWeekday.Thursday,
            DayOfWeek.Friday => DiscordScheduledEventRecurrenceWeekday.Friday,
            DayOfWeek.Saturday => DiscordScheduledEventRecurrenceWeekday.Saturday,
            DayOfWeek.Sunday => DiscordScheduledEventRecurrenceWeekday.Sunday,
            _ => throw new NotImplementedException("There are seven weekdays.")
        };
    }
}
