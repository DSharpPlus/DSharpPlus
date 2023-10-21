// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Models.Serialization.Converters;

public sealed class SnowflakeConverter : JsonConverter<Snowflake>
{
    public override Snowflake Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => new(reader.GetInt64()),
            JsonTokenType.String => new(long.Parse(reader.GetString()!, CultureInfo.InvariantCulture)),
            _ => throw new JsonException("The present payload could not be parsed as a snowflake.")
        };
    }

    public override void Write(Utf8JsonWriter writer, Snowflake value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.Value);
}
