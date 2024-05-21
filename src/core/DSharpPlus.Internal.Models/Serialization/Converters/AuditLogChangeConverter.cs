// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models.Serialization.Converters;

/// <summary>
/// Provides conversion for <see cref="IAuditLogChange"/>s.
/// </summary>
public class AuditLogChangeConverter : JsonConverter<IAuditLogChange>
{
    /// <inheritdoc/>
    public override IAuditLogChange? Read
    (
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("There was no JSON object found.");
        }

        if
        (
            !JsonDocument.TryParseValue(ref reader, out JsonDocument? document)
            || !document.RootElement.TryGetProperty("key", out JsonElement propertyKey)
        )
        {
            throw new JsonException("The provided JSON object was malformed.");
        }

        string key = propertyKey.GetString()!;

        Optional<string> newProperty = document.RootElement.TryGetProperty("new_value", out JsonElement value)
            ? JsonSerializer.Serialize(value, options)
            : new Optional<string>();

        Optional<string> oldProperty = document.RootElement.TryGetProperty("old_value", out JsonElement oldValue)
            ? JsonSerializer.Serialize(oldValue, options)
            : new Optional<string>();

        document.Dispose();

        return new AuditLogChange
        {
            Key = key,
            NewValue = newProperty,
            OldValue = oldProperty
        };
    }

    /// <inheritdoc/>
    public override void Write
    (
        Utf8JsonWriter writer,
        IAuditLogChange value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        writer.WritePropertyName("key");
        writer.WriteStringValue(value.Key);

        if (value.NewValue.TryGetValue(out string? newValue))
        {
            writer.WritePropertyName("new_value");
            writer.WriteStringValue(newValue);
        }

        if (value.OldValue.TryGetValue(out string? oldValue))
        {
            writer.WritePropertyName("old_value");
            writer.WriteStringValue(oldValue);
        }

        writer.WriteEndObject();
    }
}
