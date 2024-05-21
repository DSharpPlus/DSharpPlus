// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Models.Serialization.Converters;

/// <summary>
/// Enables <see cref="DiscordApplicationIntegrationType"/> to be used as a dictionary key.
/// </summary>
public class ApplicationIntegrationTypeKeyConverter : JsonConverter<DiscordApplicationIntegrationType>
{
    // short-circuit read and write

    /// <inheritdoc/>
    public override DiscordApplicationIntegrationType Read
    (
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
        => JsonSerializer.Deserialize<DiscordApplicationIntegrationType>(ref reader, options);

    /// <inheritdoc/>
    public override void Write
    (
        Utf8JsonWriter writer,
        DiscordApplicationIntegrationType value,
        JsonSerializerOptions options
    )
        => writer.WriteNumberValue((int)value);

    // what we actually want to override is Read/WriteAsPropertyName

    /// <inheritdoc/>
    public override DiscordApplicationIntegrationType ReadAsPropertyName
    (
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType is not JsonTokenType.PropertyName)
        {
            throw new JsonException("Expected a property name.");
        }

        string? name = reader.GetString();

        return !int.TryParse(name, CultureInfo.InvariantCulture, out int value)
            ? throw new JsonException("Expected an integer key.")
            : (DiscordApplicationIntegrationType)value;
    }

    /// <inheritdoc/>
    public override void WriteAsPropertyName
    (
        Utf8JsonWriter writer,

        [DisallowNull]
        DiscordApplicationIntegrationType value,
        JsonSerializerOptions options
    )
        => writer.WritePropertyName(((int)value).ToString(CultureInfo.InvariantCulture));
}
