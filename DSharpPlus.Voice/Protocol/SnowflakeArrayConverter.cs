using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol;

internal sealed class SnowflakeArrayConverter : JsonConverter<IReadOnlyList<ulong>>
{
    public override IReadOnlyList<ulong>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        List<ulong> snowflakes = [];

        reader.Read();

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                snowflakes.Add(reader.GetUInt64());
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                string snowflake = reader.GetString();
                snowflakes.Add(ulong.Parse(snowflake));
            }
            else
            {
                throw new JsonException($"Expected snowflake, got token type {reader.TokenType}.");
            }

            reader.Read();
        }

        return snowflakes;
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyList<ulong> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (ulong snowflake in value)
        {
            writer.WriteNumberValue(snowflake);
        }

        writer.WriteEndArray();
    }
}
