// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

using DSharpPlus.Collections;

namespace DSharpPlus.Converters;

/// <summary>
/// Provides JSON conversion logic for <see cref="DictionarySlim{TKey, TValue}"/> with <see cref="IParsable{TSelf}"/>
/// keys. It is assumed that there is a correct roundtrip between <see cref="object.ToString"/> and
/// <see cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>.
/// </summary>
[RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed.")]
[RequiresDynamicCode("JSON serialization and deserialization might need runtime code generation. Use the source generator instead.")]
public class DictionarySlimIParsableTValueJsonConverter<TKey, TValue> : JsonConverter<DictionarySlim<TKey, TValue>>
    where TKey : IEquatable<TKey>, IParsable<TKey>
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

            if(!TKey.TryParse(propertyNameRaw, null, out TKey? propertyName))
            {
                throw new ArgumentException($"The key {propertyNameRaw} could not be parsed as {typeof(TKey)}.");
            }

            reader.Read();

            ref TValue propertyValue = ref result.GetOrAddValueRef(propertyName!);

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
