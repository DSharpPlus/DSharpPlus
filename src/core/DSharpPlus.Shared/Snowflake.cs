// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable CA5394

using System;

namespace DSharpPlus;

/// <summary>
/// Represents a discord snowflake; the type discord uses for IDs first and foremost.
/// </summary>
public readonly partial record struct Snowflake :
    IComparable<Snowflake>
{
    /// <summary>
    /// The discord epoch; the start of 2015. All snowflakes are based upon this time.
    /// </summary>
    public static readonly DateTimeOffset DiscordEpoch = new(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// The snowflake's underlying value.
    /// </summary>
    public long Value { get; }

    /// <summary>
    /// The time when this snowflake was created.
    /// </summary>
    public DateTimeOffset Timestamp => DiscordEpoch.AddMilliseconds
    (
        this.Value >> 22
    );

    /// <summary>
    /// The internal worker's ID that was used to generate the snowflake.
    /// </summary>
    public byte InternalWorkerId => (byte)((this.Value & 0x3E0000) >> 17);

    /// <summary>
    /// The internal process' ID that was used to generate the snowflake.
    /// </summary>
    public byte InternalProcessId => (byte)((this.Value & 0x1F000) >> 12);

    /// <summary>
    /// The internal worker-specific and process-specific increment.
    /// </summary>
    public ulong InternalIncrement => (ulong)(this.Value & 0xFFF);

    /// <summary>
    /// Creates a new snowflake from a given integer.
    /// </summary>
    /// <param name="value">The numerical representation to translate from.</param>
    public Snowflake(long value)
        => this.Value = value;

    /// <summary>
    /// Creates a fake snowflake from scratch. If no parameters are provided, returns a newly generated snowflake.
    /// </summary>
    /// <remarks>
    /// If a value larger than allowed is supplied for the three numerical parameters, it will be cut off at
    /// the maximum allowed value.
    /// </remarks>
    /// <param name="timestamp">
    /// The date when the snowflake was created. If null, this defaults to the current time.
    /// </param>
    /// <param name="workerId">
    /// A 5 bit worker id that was used to create the snowflake. If null, generates a random number between 0 and 31.
    /// </param>
    /// <param name="processId">
    /// A 5 bit process id that was used to create the snowflake. If null, generates a random number between 0 and 31.
    /// </param>
    /// <param name="increment">
    /// A 12 bit integer which represents the number of previously generated snowflakes in the given context.
    /// If null, generates a random number between 0 and 4,095.
    /// </param>
    public Snowflake
    (
        DateTimeOffset? timestamp = null,
        byte? workerId = null,
        byte? processId = null,
        ushort? increment = null
    )
    {
        timestamp ??= DateTimeOffset.Now;
        workerId ??= (byte)Random.Shared.Next(0, 32);
        processId ??= (byte)Random.Shared.Next(0, 32);
        increment ??= (ushort)Random.Shared.Next(0, 4095);

        this.Value = ((long)timestamp.Value.Subtract(DiscordEpoch).TotalMilliseconds << 22)
            | ((long)workerId.Value << 17)
            | ((long)processId.Value << 12)
            | increment.Value;
    }

    public int CompareTo(Snowflake other)
        => this.Value.CompareTo(other.Value);

    public static bool operator <(Snowflake left, Snowflake right) => left.Value < right.Value;
    public static bool operator <=(Snowflake left, Snowflake right) => left.Value <= right.Value;
    public static bool operator >(Snowflake left, Snowflake right) => left.Value > right.Value;
    public static bool operator >=(Snowflake left, Snowflake right) => left.Value >= right.Value;
    public static implicit operator long(Snowflake snowflake) => snowflake.Value;
    public static implicit operator Snowflake(long value) => value;
}
