// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Globalization;
using DSharpPlus.Core.JsonConverters;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    // TODO: Write a validation test.
    /// <summary>
    /// Implements https://discord.com/developers/docs/reference#snowflakes.
    /// </summary>
    [JsonConverter(typeof(DiscordSnowflakeConverter))]
    public sealed record DiscordSnowflake : IComparable<DiscordSnowflake>
    {
        /// <summary>
        /// The first second of 2015. The date Discord officially recognizes as it's epoch.
        /// </summary>
        public static readonly DateTimeOffset DiscordEpoch = new(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// A numerical representation of the snowflake.
        /// </summary>
        public ulong Value { get; init; }

        /// <summary>
        /// Milliseconds since Discord Epoch, the first second of 2015 or 1420070400000.
        /// </summary>
        public DateTimeOffset Timestamp { get; init; }

        /// <summary>
        /// The internal worker's ID that was used to generate the snowflake.
        /// </summary>
        public byte InternalWorkerId { get; init; }

        /// <summary>
        /// The internal process' ID that was used to generate the snowflake.
        /// </summary>
        public byte InternalProcessId { get; init; }

        /// <summary>
        /// A number incremented by 1 every time the snowflake is generated.
        /// </summary>
        public ushort InternalIncrement { get; init; }

        /// <summary>
        /// Creates a new snowflake from a numerical representation.
        /// </summary>
        /// <param name="value">The numerical representation to translate from.</param>
        public DiscordSnowflake(ulong value)
        {
            Value = value;
            Timestamp = DiscordEpoch.AddMilliseconds(value >> 22);
            InternalWorkerId = (byte)((value & 0x3E0000) >> 17);
            InternalProcessId = (byte)((value & 0x1F000) >> 12);
            InternalIncrement = (ushort)(value & 0xFFF);
        }

        /// <summary>
        /// Creates a fake snowflake from scratch. If no parameters are provided, returns a randomly generated snowflake.
        /// </summary>
        /// <param name="timestamp">The date when the snowflake was created at. If null, defaults to the current time.</param>
        /// <param name="workerId">A 5 bit worker id that was used to create the snowflake. If null, generates a random number between 1 and 31.</param>
        /// <param name="processId">A 5 bit process id that was used to create the snowflake. If null, generates a random number between 1 and 31.</param>
        /// <param name="increment">A 12 bit integer which represents the number of previously generated snowflakes. If null, generates a random number between 1 and 4,095.</param>
        public DiscordSnowflake(DateTimeOffset? timestamp, byte? workerId, byte? processId, ushort? increment)
        {
            timestamp ??= DateTimeOffset.Now;
            workerId ??= (byte)Random.Shared.Next(1, 32);
            processId ??= (byte)Random.Shared.Next(1, 32);
            increment ??= (ushort)Random.Shared.Next(1, 4095);

            Timestamp = timestamp.Value;
            InternalWorkerId = workerId.Value;
            InternalProcessId = processId.Value;
            InternalIncrement = increment.Value;

            Value = (((uint)timestamp.Value.Subtract(DiscordEpoch).TotalMilliseconds) << 22)
                | ((ulong)workerId.Value << 17)
                | ((ulong)processId.Value << 12)
                | increment.Value;
        }

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        public override int GetHashCode() => HashCode.Combine(Value, Timestamp, InternalWorkerId, InternalProcessId, InternalIncrement);
        public int CompareTo(DiscordSnowflake? other) => other is null ? 1 : Value.CompareTo(other.Value);

        public static bool operator <(DiscordSnowflake left, DiscordSnowflake right) => left is null ? right is not null : left.CompareTo(right) < 0;
        public static bool operator <=(DiscordSnowflake left, DiscordSnowflake right) => left is null || left.CompareTo(right) <= 0;
        public static bool operator >(DiscordSnowflake left, DiscordSnowflake right) => left is not null && left.CompareTo(right) > 0;
        public static bool operator >=(DiscordSnowflake left, DiscordSnowflake right) => left is null ? right is null : left.CompareTo(right) >= 0;
        public static implicit operator ulong(DiscordSnowflake snowflake) => snowflake.Value;
        public static implicit operator DiscordSnowflake(ulong value) => new(value);
    }
}
