// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus;

public readonly partial record struct Snowflake
{
    public static Snowflake operator +
    (
        Snowflake left,
        TimeSpan right
    )
    {
        long time = (long)right.TotalMilliseconds << 22;
        return left.Value + time;
    }

    public static Snowflake operator -
    (
        Snowflake left,
        TimeSpan right
    )
    {
        long time = (long)right.TotalMilliseconds << 22;
        return left.Value - time;
    }

    public static bool operator ==
    (
        Snowflake left,
        DateTimeOffset right
    )
        => left.Timestamp == right;

    public static bool operator !=
    (
        Snowflake left,
        DateTimeOffset right
    )
        => left.Timestamp != right;

    public static bool operator <
    (
        Snowflake left,
        DateTimeOffset right
    )
        => left.Timestamp < right;

    public static bool operator <=
    (
        Snowflake left,
        DateTimeOffset right
    )
        => left.Timestamp <= right;

    public static bool operator >
    (
        Snowflake left,
        DateTimeOffset right
    )
        => left.Timestamp > right;

    public static bool operator >=
    (
        Snowflake left,
        DateTimeOffset right
    )
        => left.Timestamp >= right;

    /// <summary>
    /// Returns the absolute difference in time between the two snowflakes.
    /// </summary>
    public static TimeSpan GetAbsoluteTimeDifference
    (
        Snowflake first,
        Snowflake second
    )
    {
        long absolute = long.Abs
        (
            first - second
        );

        return new
        (
            (absolute >> 22) * 10_000
        );
    }

    /// <summary>
    /// Creates a new snowflake from an offset into the future.
    /// </summary>
    public static Snowflake FromFuture
    (
        TimeSpan offset
    )
    {
        return new
        (
            DateTimeOffset.UtcNow + offset,
            0,
            0,
            0
        );
    }

    /// <summary>
    /// Creates a new snowflake from an offset into the past.
    /// </summary>
    public static Snowflake FromPast
    (
        TimeSpan offset
    )
    {
        return new
        (
            DateTimeOffset.UtcNow - offset,
            0,
            0,
            0
        );
    }
}
