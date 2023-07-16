// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#if !NETSTANDARD

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using DSharpPlus.Collections;

namespace DSharpPlus.Converters;

/// <summary>
/// Represents a specialized converter for string-to-string slim dictionaries. This converter is AOT-friendly.
/// </summary>
public class DictionarySlimStringStringJsonConverter : JsonConverter<DictionarySlim<string, string>>
{
    /// <inheritdoc/>
    public override DictionarySlim<string, string>? Read
    (
        ref Utf8JsonReader reader, 
        Type typeToConvert, 
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("The provided dictionary start token was not an object.");
        }

        DictionarySlim<string, string> result = new();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return result;
            }

            string propertyName = reader.GetString()!;

            reader.Read();

            ref string propertyValue = ref result.GetOrAddValueRef(propertyName);

            propertyValue = reader.GetString()!;
        }

        return result;
    }

    /// <inheritdoc/>
    public override void Write
    (
        Utf8JsonWriter writer, 
        DictionarySlim<string, string> value, 
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        foreach (KeyValuePair<string, string> element in value)
        {
            writer.WritePropertyName(element.Key);
            writer.WriteStringValue(element.Value);
        }

        writer.WriteEndObject();
    }
}

#endif
