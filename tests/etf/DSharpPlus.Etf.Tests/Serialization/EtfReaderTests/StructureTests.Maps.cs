// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Etf.Serialization;

using Xunit;

namespace DSharpPlus.Etf.Tests.Serialization.EtfReaderTests;

partial class StructureTests
{
    private static readonly byte[] mapPayload =
    [
        // ETF version header
        0x83,

        // map header
        0x74, 0x00, 0x00, 0x00, 0x01,

        // key
        0x61, 0x07,

        // value
        0x61, 0x08
    ];

    [Fact]
    public void TestReadingMap()
    {
        ReadOnlySpan<byte> span = [.. mapPayload];
        EtfReader reader = new
        (
            span,
            stackalloc uint[1],
            stackalloc TermType[1]
        );

        Assert.True(reader.Read());
        Assert.Equal(TermType.Map, reader.TermType);
        Assert.Equal(EtfTokenType.StartMap, reader.TokenType);

        // key
        Assert.True(reader.Read());
        Assert.Equal((uint)1, reader.GetCurrentRemainingLength());

        // value
        _ = reader.Read();

        // are we correctly synthesizing the last end token, even if there is no actual data left?
        Assert.True(reader.Read());
        Assert.Equal(EtfTokenType.EndMap, reader.TokenType);
    }
}
