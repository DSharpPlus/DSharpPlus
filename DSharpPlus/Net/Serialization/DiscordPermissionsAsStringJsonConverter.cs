namespace DSharpPlus.Net.Serialization;

using System;
using System.Globalization;

using Newtonsoft.Json;

/// <summary>
/// Facilitates serializing permissions as string.
/// </summary>
internal sealed class DiscordPermissionsAsStringJsonConverter : JsonConverter<Permissions>
{
    public override Permissions ReadJson
    (
        JsonReader reader,
        Type objectType,
        Permissions existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        string? value = reader.ReadAsString();

        return value is not null ? (Permissions)ulong.Parse(value) : existingValue;
    }

    public override void WriteJson(JsonWriter writer, Permissions value, JsonSerializer serializer)
        => writer.WriteValue(((ulong)value).ToString(CultureInfo.InvariantCulture));
}
