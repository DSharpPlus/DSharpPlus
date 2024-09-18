using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Serialization.Json.Converters;

/// <summary>
/// Provides a per-property byte array converter for cases where discord elects to not play by the recommendations of
/// RFC 7493, Section 4.4.
/// </summary>
internal sealed class ByteArrayConverter : JsonConverter<byte[]>
{
    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!JsonDocument.TryParseValue(ref reader, out JsonDocument? document))
        {
            throw new JsonException("Encountered invalid JSON");
        }

        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Expected a byte array");
        }

        byte[] buffer = new byte[document.RootElement.GetArrayLength()];
        int index = 0;

        foreach (JsonElement element in document.RootElement.EnumerateArray())
        {
            buffer[index] = element.GetByte();
            index++;
        }

        document.Dispose();

        return buffer;
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (byte b in value)
        {
            writer.WriteNumberValue(b);
        }

        writer.WriteEndArray();
    }
}
