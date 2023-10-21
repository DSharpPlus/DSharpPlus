// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;

using DSharpPlus.Core.Models.Serialization.Converters;

using OneOf;

using Xunit;

namespace DSharpPlus.Core.Models.Tests.Converters;

// here we test whether we handle snowflake precedence correctly, with intentionally annoying unions
public partial class OneOfConverterTests
{
    private static ReadOnlySpan<byte> SnowflakeIntegerPayload => "\"983987834938\""u8;

    [Fact]
    public void TestSnowflakePrecedence()
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
}
