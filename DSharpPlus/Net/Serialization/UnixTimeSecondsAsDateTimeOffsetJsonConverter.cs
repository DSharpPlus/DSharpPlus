using System;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.Serialization;

/// <summary>
/// Json converter for de/serializing DateTimeOffset as unix time in seconds.
/// </summary>
internal sealed class UnixTimeSecondsAsDateTimeOffsetJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(DateTimeOffset);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JToken jr = JToken.Load(reader);

        long unixTimestamp = jr.ToObject<long>();
        return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        long unixTimestamp = ((DateTimeOffset)value!).ToUnixTimeSeconds();
        writer.WriteValue(unixTimestamp.ToString(CultureInfo.InvariantCulture));
    }
}
