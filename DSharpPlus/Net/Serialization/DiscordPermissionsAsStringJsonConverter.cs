
using System;
using System.Globalization;

using DSharpPlus.Entities;

using Newtonsoft.Json;

namespace DSharpPlus.Net.Serialization;
/// <summary>
/// Facilitates serializing permissions as string.
/// </summary>
internal sealed class DiscordPermissionsAsStringJsonConverter : JsonConverter<DiscordPermissions>
{
    public override DiscordPermissions ReadJson
    (
        JsonReader reader,
        Type objectType,
        DiscordPermissions existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        string? value = reader.ReadAsString();

        return value is not null ? (DiscordPermissions)ulong.Parse(value) : existingValue;
    }

    public override void WriteJson(JsonWriter writer, DiscordPermissions value, JsonSerializer serializer)
    {
        if ((ulong)value == 0)
        {
            writer.WriteNull();
        }
        else
        {
            writer.WriteValue(((ulong)value).ToString(CultureInfo.InvariantCulture));
        }
    }
}
