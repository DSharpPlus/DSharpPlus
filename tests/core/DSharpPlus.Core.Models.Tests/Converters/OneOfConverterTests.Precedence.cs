// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;

using DSharpPlus.Core.Models.Serialization.Converters;

using OneOf;

using Xunit;

namespace DSharpPlus.Core.Models.Tests.Converters;

// here we test whether we handle precedence correctly, with intentionally annoying unions
public partial class OneOfConverterTests
{
    private static ReadOnlySpan<byte> SnowflakeIntegerPayload => "983987834938"u8;
    private static ReadOnlySpan<byte> SnowflakeStringPayload => "\"737837872\""u8;
    private static ReadOnlySpan<byte> IntegerFloatPayload => "887673"u8;

    [Fact]
    public void TestSnowflakeLongPrecedence()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new OneOfConverterFactory());
        options.Converters.Add(new SnowflakeConverter());

        OneOf<Snowflake, ulong> union = JsonSerializer.Deserialize<OneOf<Snowflake, ulong>>
        (
            SnowflakeIntegerPayload,
            options
        );

        Assert.True(union.IsT0);

        OneOf<ulong, Snowflake> otherUnion = JsonSerializer.Deserialize<OneOf<ulong, Snowflake>>
        (
            SnowflakeIntegerPayload,
            options
        );

        Assert.True(otherUnion.IsT1);
    }

    [Fact]
    public void TestSnowflakeStringPrecedence()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new OneOfConverterFactory());
        options.Converters.Add(new SnowflakeConverter());

        OneOf<Snowflake, string> union = JsonSerializer.Deserialize<OneOf<Snowflake, string>>
        (
            SnowflakeStringPayload,
            options
        );

        Assert.True(union.IsT0);

        OneOf<string, Snowflake> otherUnion = JsonSerializer.Deserialize<OneOf<string, Snowflake>>
        (
            SnowflakeStringPayload,
            options
        );

        Assert.True(otherUnion.IsT1);
    }

    [Fact]
    public void TestIntegerFloatPrecedence()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new OneOfConverterFactory());

        OneOf<ulong, double> union = JsonSerializer.Deserialize<OneOf<ulong, double>>
        (
            IntegerFloatPayload,
            options
        );

        Assert.True(union.IsT0);

        OneOf<double, ulong> otherUnion = JsonSerializer.Deserialize<OneOf<double, ulong>>
        (
            IntegerFloatPayload,
            options
        );

        Assert.True(otherUnion.IsT1);
    }
}
