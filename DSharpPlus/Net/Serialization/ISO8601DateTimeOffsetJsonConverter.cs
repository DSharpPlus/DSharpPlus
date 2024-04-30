using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace DSharpPlus.Net.Serialization;

/// <summary>
/// Json converter for handling DateTimeOffset values.
/// </summary>
internal sealed class ISO8601DateTimeOffsetJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        => writer.WriteValue(((DateTimeOffset)value).ToString("O", CultureInfo.InvariantCulture));
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JToken jr = JToken.Load(reader);

        return jr.ToObject<DateTimeOffset>();
    }
    public override bool CanConvert(Type objectType) => objectType == typeof(DateTimeOffset);
}
