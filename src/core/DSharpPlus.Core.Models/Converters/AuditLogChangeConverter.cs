// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models.Converters;

/// <summary>
/// Provides conversion for <seealso cref="IAuditLogChange"/>s.
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
            || document.RootElement.TryGetProperty("key", out JsonElement propertyKey)
            || document.RootElement.TryGetProperty("new_value", out JsonElement newProperty)
            || document.RootElement.TryGetProperty("old_value", out JsonElement oldProperty)
        )
        {
            throw new JsonException("The provided JSON object was malformed.");
        }

        string key = propertyKey.GetString()!;

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

        writer.WritePropertyName("new_value");
        value.NewValue.WriteTo(writer);

        writer.WritePropertyName("old_value");
        value.OldValue.WriteTo(writer);

        writer.WriteEndObject();
    }
}
