// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#if !NETSTANDARD

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using DSharpPlus.Collections;

namespace DSharpPlus.Converters;

/// <summary>
/// Provides JSON serialization with the appropriate converters for slim dictionaries.
/// </summary>
[RequiresDynamicCode("Monomorphized generic instantiations of DictionarySlim<TKey, TValue> might have been trimmed.")]
[RequiresUnreferencedCode("JSON serialization and deserialization might require static analysis of trimmed types.")]
public class DictionarySlimJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert
    (
        Type typeToConvert
    )
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        if (typeToConvert.GetGenericTypeDefinition() != typeof(DictionarySlim<,>))
        {
            return false;
        }

        Type[] generics = typeToConvert.GetGenericArguments();

        return VerifyKeyTypeValidity(generics[0]);
    }

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter
    (
        Type typeToConvert, 
        JsonSerializerOptions options
    )
    {
        Type[] generics = typeToConvert.GetGenericArguments();

        // we can add more specialized converters here for more AOT-designed, specialized converters here,
        // though only if they add new key types or specialize TValue to a meaningful degree. specializing
        // on TKey where TKey : IParsable<TKey> makes little sense, for the CLR implementation.
        if (generics[0] == typeof(string) && generics[1] == typeof(string))
        {
            return new DictionarySlimStringStringJsonConverter();
        }
        else if (generics[0] == typeof(string))
        {
            Type converter = typeof(DictionarySlimStringTValueJsonConverter<>)
                .MakeGenericType(generics[1]);

            return (JsonConverter?)Activator.CreateInstance(converter);
        }
        else if 
        (
            generics[0].GetInterfaces()
            .Any
            (
                candidate => candidate.IsGenericType 
                    && candidate.GetGenericTypeDefinition() == typeof(IParsable<>)
            )
        )
        {
            Type converter = typeof(DictionarySlimIParsableTValueJsonConverter<,>)
                .MakeGenericType(generics);

            return (JsonConverter?)Activator.CreateInstance(converter);
        }
        else
        {
            Type converter = typeof(DictionarySlimGenericJsonConverter<,>)
                .MakeGenericType(generics);

            return (JsonConverter?)Activator.CreateInstance(converter);
        }
    }

    private static bool VerifyKeyTypeValidity
    (
        Type type
    )
    { 
        return type == typeof(string) || type.GetInterfaces()
        .Any
        (
            candidate => candidate.IsGenericType
                && candidate.GetGenericTypeDefinition() == typeof(IParsable<>)
        );
    }
}

#endif
