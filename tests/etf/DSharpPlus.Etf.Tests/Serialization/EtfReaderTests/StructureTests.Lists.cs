// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Etf.Serialization;

using Xunit;

namespace DSharpPlus.Etf.Tests.Serialization.EtfReaderTests;

/// <summary>
/// Contains tests for correctly reading control structures.
/// </summary>
public partial class StructureTests
{
    private static readonly byte[] listPayload =
    [
        // ETF version header
        0x83,

        // list introduction
        0x6C, 0x00, 0x00, 0x00, 0x02,

            // first child etf term
            0x62, 0x00, 0x00, 0x01, 0x00,

            // second child etf term
            0x62, 0x00, 0x00, 0x00, 0xFF,

            // null terminator
            0x6A,

        // term that isn't a child term
        0x61, 0x07
    ];

    [Fact]
    public void TestReadingList()
    {
        ReadOnlySpan<byte> span = [.. listPayload];
        EtfReader reader = new
        (
            span,
            stackalloc uint[1],
            stackalloc TermType[1]
        );

        Assert.True(reader.Read());
        Assert.Equal(TermType.List, reader.TermType);
        Assert.Equal(EtfTokenType.StartList, reader.TokenType);

        // the expected length here is 3: two items and the null terminator
        // the null terminator doesn't actually need to be null, either, as per the specification, soo...
        // https://www.erlang.org/doc/apps/erts/erl_ext_dist#list_ext
        Assert.Equal((uint)3, reader.GetCurrentRemainingLength());

        // test whether we correctly revert back to EtfTokenType.Term
        Assert.True(reader.Read());
        Assert.Equal(EtfTokenType.Term, reader.TokenType);

        _ = reader.Read();

        // are we null terminating correctly?
        Assert.True(reader.Read());
        Assert.Equal(TermType.Nil, reader.TermType);
        Assert.Equal((uint)0, reader.GetCurrentRemainingLength());

        // are we synthesizing reads correctly
        Assert.True(reader.Read());
        Assert.Equal(EtfTokenType.EndList, reader.TokenType);

        // are we reading the final term correctly?
        Assert.True(reader.Read());
        Assert.Equal(TermType.SmallInteger, reader.TermType);
    }
}
