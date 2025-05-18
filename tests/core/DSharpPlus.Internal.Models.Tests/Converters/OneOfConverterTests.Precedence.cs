// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;
using System.Threading.Tasks;

using DSharpPlus.Internal.Models.Serialization.Converters;

using OneOf;

namespace DSharpPlus.Internal.Models.Tests.Converters;

// here we test whether we handle precedence correctly, with intentionally annoying unions
public partial class OneOfConverterTests
{
    private static ReadOnlySpan<byte> SnowflakeIntegerPayload => "983987834938"u8;
    private static ReadOnlySpan<byte> SnowflakeStringPayload => "\"737837872\""u8;
    private static ReadOnlySpan<byte> IntegerFloatPayload => "887673"u8;

    [Test]
    public async Task TestSnowflakeLongPrecedence()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new OneOfConverterFactory());
        options.Converters.Add(new SnowflakeConverter());

        OneOf<Snowflake, ulong> union = JsonSerializer.Deserialize<OneOf<Snowflake, ulong>>
        (
            SnowflakeIntegerPayload,
            options
        );

        await Assert.That(union.IsT0).IsTrue();

        OneOf<ulong, Snowflake> otherUnion = JsonSerializer.Deserialize<OneOf<ulong, Snowflake>>
        (
            SnowflakeIntegerPayload,
            options
        );

        await Assert.That(otherUnion.IsT1).IsTrue();
    }

    [Test]
    public async Task TestSnowflakeStringPrecedence()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new OneOfConverterFactory());
        options.Converters.Add(new SnowflakeConverter());

        OneOf<Snowflake, string> union = JsonSerializer.Deserialize<OneOf<Snowflake, string>>
        (
            SnowflakeStringPayload,
            options
        );

        await Assert.That(union.IsT0).IsTrue();

        OneOf<string, Snowflake> otherUnion = JsonSerializer.Deserialize<OneOf<string, Snowflake>>
        (
            SnowflakeStringPayload,
            options
        );

        await Assert.That(otherUnion.IsT1).IsTrue();
    }

    [Test]
    public async Task TestIntegerFloatPrecedence()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new OneOfConverterFactory());

        OneOf<ulong, double> union = JsonSerializer.Deserialize<OneOf<ulong, double>>
        (
            IntegerFloatPayload,
            options
        );

        await Assert.That(union.IsT0).IsTrue();

        OneOf<double, ulong> otherUnion = JsonSerializer.Deserialize<OneOf<double, ulong>>
        (
            IntegerFloatPayload,
            options
        );

        await Assert.That(otherUnion.IsT1).IsTrue();
    }
}
