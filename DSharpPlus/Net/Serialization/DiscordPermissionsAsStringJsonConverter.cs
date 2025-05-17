using System;
using System.Numerics;
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
        string? value = reader.Value as string;

        return value is not null ? new(BigInteger.Parse(value)) : existingValue;
    }

    public override void WriteJson(JsonWriter writer, DiscordPermissions value, JsonSerializer serializer)
    {
        if (value == DiscordPermissions.None)
        {
            writer.WriteNull();
        }
        else
        {
            writer.WriteValue(value.ToString());
        }
    }
}
