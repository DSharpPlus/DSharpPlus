using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol;

internal sealed class SnowflakeConverter : JsonConverter<ulong>
{
    public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();

        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetUInt64();
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            string snowflake = reader.GetString();
            return ulong.Parse(snowflake);
        }
        else
        {
            throw new JsonException($"Expected snowflake, got token type {reader.TokenType}.");
        }
    }

    public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
