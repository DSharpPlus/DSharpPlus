// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Etf.Serialization;

using Xunit;

namespace DSharpPlus.Etf.Tests.Serialization.EtfReaderTests;

partial class PrimitiveTests
{
    private static readonly byte[] floatPayload =
    [
        // etf version header
        0x83,

        // new float term
        0x46, 0x40, 0x09, 0x21, 0xCA, 0xC0, 0x83, 0x12, 0x6F,

        // old float term
        0x63, 0x33, 0x2E, 0x31, 0x34, 0x31, 0x35, 0x39, 0x32, 0x36, 0x35, 0x33, 0x35, 0x38, 0x39, 0x37,
        0x39, 0x33, 0x32, 0x33, 0x38, 0x34, 0x36, 0x32, 0x36, 0x34, 0x33, 0x33, 0x38, 0x33, 0x32, 0x37
    ];

    /// <summary>
    /// Tests whether reading a new float works correctly.
    /// </summary>
    [Fact]
    public void TestReadingNewFloat()
    {
        ReadOnlySpan<byte> span = [.. floatPayload];
        EtfReader reader = new
        (
            span,
            stackalloc uint[1],
            stackalloc TermType[1]
        );

        Assert.True(reader.Read());

        Assert.Equal(3.1415, reader.ReadDouble(), 0.0001);
        Assert.True(reader.TryReadHalf(out Half result));
    }

    /// <summary>
    /// Tests whether reading an old float works correctly.
    /// </summary>
    [Fact]
    public void TestReadingOldFloat()
    {
        ReadOnlySpan<byte> span = [.. floatPayload];
        EtfReader reader = new
        (
            span,
            stackalloc uint[1],
            stackalloc TermType[1]
        );

        _ = reader.Read();

        Assert.True(reader.Read());

        Assert.Equal(3.14159265358979323846264338327, reader.ReadDouble(), 0.0001);
        Assert.True(reader.TryReadSingle(out float result));
    }
}
