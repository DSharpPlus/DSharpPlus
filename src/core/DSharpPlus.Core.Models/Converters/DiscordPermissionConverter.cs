// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Models.Converters;

/// <summary>
/// Enables serializing and deserializing Discord permissions.
/// </summary>
public class DiscordPermissionConverter : JsonConverter<DiscordPermissions>
{
    /// <inheritdoc/>
    public override DiscordPermissions Read
    (
        ref Utf8JsonReader reader, 
        Type typeToConvert, 
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType is not JsonTokenType.String or JsonTokenType.Number)
        {
            throw new JsonException("The provided permission value was provided in an unrecognized format.");
        }

        string raw = reader.GetString()!;

        return !ulong.TryParse(raw, out ulong permissions)
            ? throw new JsonException("The provided permission value could not be parsed.")
            : (DiscordPermissions)permissions;
    }

    /// <inheritdoc/>
    public override void Write
    (
        Utf8JsonWriter writer,
        DiscordPermissions value,
        JsonSerializerOptions options
    ) 
        => writer.WriteStringValue(((ulong)value).ToString(CultureInfo.InvariantCulture));
}
