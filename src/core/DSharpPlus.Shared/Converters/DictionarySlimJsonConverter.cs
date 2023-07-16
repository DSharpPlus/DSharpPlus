// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#if !NETSTANDARD

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using DSharpPlus.Collections;

namespace DSharpPlus.Converters;

[RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed.")]
[RequiresDynamicCode("JSON serialization and deserialization might need runtime code generation. Use the source generator instead.")]
public class DictionarySlimJsonConverter<TKey, TValue> : JsonConverter<DictionarySlim<TKey, TValue>>
    where TKey : IEquatable<TKey>
{
    /// <inheritdoc/>
    public override DictionarySlim<TKey, TValue>? Read
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

        DictionarySlim<TKey, TValue> result = new();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return result;
            }

            string propertyNameRaw = reader.GetString()!;

            TKey propertyName;

            // TKey is a string, return a string
            if (typeof(TKey) == typeof(string))
            {
                propertyName = (TKey)(object)propertyNameRaw;
            }
            else if (typeof(TKey).IsAssignableTo(typeof(IParsable<>)))
            {
                // TODO: cache these
                object parsed = typeof(TKey).GetMethod
                (
                    "Parse",
                    BindingFlags.Public | BindingFlags.Static
                )!
                .Invoke
                (
                    null,
                    new object?[]
                    {
                        propertyNameRaw,
                        null
                    }
                )!;

                propertyName = (TKey)parsed;
            }
            else
            {
                propertyName = JsonSerializer.Deserialize<TKey>(propertyNameRaw)!;
            }

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
        DictionarySlim<TKey, TValue> value, 
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        foreach (KeyValuePair<TKey, TValue> element in value)
        {
            writer.WritePropertyName(element.Key.ToString()!);
            writer.WriteRawValue(JsonSerializer.SerializeToUtf8Bytes(element.Value, options));
        }

        writer.WriteEndObject();
    }
}

#endif
