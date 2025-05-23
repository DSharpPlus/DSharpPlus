// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0072

using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Models.Serialization.Converters;

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
        return reader.TokenType switch
        {
            JsonTokenType.String => !BigInteger.TryParse(reader.GetString()!, out BigInteger permissions)
                ? throw new JsonException("The provided permission value could not be parsed.")
                : new DiscordPermissions(permissions),

            JsonTokenType.Number => new DiscordPermissions(new BigInteger(reader.GetUInt64())),
            _ => throw new JsonException("The provided permission value was provided in an unrecognized format.")
        };
    }

    /// <inheritdoc/>
    public override void Write
    (
        Utf8JsonWriter writer,
        DiscordPermissions value,
        JsonSerializerOptions options
    )
        => writer.WriteStringValue(value.ToString());
}
