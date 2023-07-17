// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#if !NETSTANDARD

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

using DSharpPlus.Collections;

namespace DSharpPlus.Converters;

/// <summary>
/// Provides JSON conversion logic for <see cref="DictionarySlim{TKey, TValue}"/> with <see langword="string"/> keys.
/// </summary>
[RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed.")]
[RequiresDynamicCode("JSON serialization and deserialization might need runtime code generation. Use the source generator instead.")]
public class DictionarySlimStringTValueJsonConverter<TValue> : JsonConverter<DictionarySlim<string, TValue>>
{
    /// <inheritdoc/>
    public override DictionarySlim<string, TValue>? Read
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

        DictionarySlim<string, TValue> result = new();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return result;
            }

            string propertyName = reader.GetString()!;

            reader.Read();

            ref TValue propertyValue = ref result.GetOrAddValueRef(propertyName);

            propertyValue = JsonSerializer.Deserialize<TValue>(ref reader, options)!;
        }

        return result;
    }

    /// <inheritdoc/>
    public override void Write
    (
        Utf8JsonWriter writer, 
        DictionarySlim<string, TValue> value, 
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        foreach (KeyValuePair<string, TValue> element in value)
        {
            writer.WritePropertyName(element.Key);
            writer.WriteRawValue(JsonSerializer.SerializeToUtf8Bytes(element.Value, options));
        }

        writer.WriteEndObject();
    }
}

#endif
