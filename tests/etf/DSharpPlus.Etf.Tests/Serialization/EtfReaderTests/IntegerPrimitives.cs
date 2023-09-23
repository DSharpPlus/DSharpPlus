// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Etf.Serialization;

using Xunit;

namespace DSharpPlus.Etf.Tests.Serialization.EtfReaderTests;

/// <summary>
/// Contains tests for the two integer primitives and reading them as other integer types.
/// </summary>
public class IntegerPrimitives
{
    private static readonly byte[] payload =
    [
        // ETF version header
        0x83,

        // 32-bit integer
        0x62, 0x00, 0x00, 0x00, 0x04,

        // 8-bit integer
        0x61, 0x07
    ];

    /// <summary>
    /// Tests reading the first 32-bit integer.
    /// </summary>
    [Fact]
    public void TestReadingInt32()
    {
        ReadOnlySpan<byte> span = [.. payload];
        EtfReader reader = new
        (
            span,
            stackalloc uint[1],
            stackalloc TermType[1]
        );

        Assert.True(reader.Read());
        Assert.Equal(TermType.Integer, reader.TermType);
        Assert.Equal(4, reader.ReadInt32());
        Assert.Equal(4, reader.ReadInt64());
        Assert.Equal(4, reader.ReadInt16());
        Assert.Equal(4UL, reader.ReadUInt64());

        Assert.True(reader.Read());
    }

    /// <summary>
    /// Tests reading the second 8-bit integer.
    /// </summary>
    [Fact]
    public void TestReadingByte()
    {
        ReadOnlySpan<byte> span = [.. payload];
        EtfReader reader = new
        (
            span,
            stackalloc uint[1],
            stackalloc TermType[1]
        );

        // discard the first term
        _ = reader.Read();

        Assert.True(reader.Read());
        Assert.Equal(TermType.SmallInteger, reader.TermType);
        Assert.Equal(7, reader.ReadInt32());
        Assert.Equal(7, reader.ReadByte());
        Assert.Equal(7, reader.ReadInt16());
        Assert.Equal(7, reader.ReadSByte());
    }
}
