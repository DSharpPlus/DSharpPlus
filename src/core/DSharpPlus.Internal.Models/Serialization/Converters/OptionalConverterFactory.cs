// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DSharpPlus.Internal.Models.Serialization.Converters;

/// <summary>
/// A converter factory for <seealso cref="OptionalConverter{T}"/>.
/// </summary>
public class OptionalConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsConstructedGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);

    public override JsonConverter? CreateConverter
    (
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return (JsonConverter)typeof(OptionalConverter<>)
            .MakeGenericType(typeToConvert.GetGenericArguments())
            .GetConstructor(Type.EmptyTypes)!
            .Invoke(null)!;
    }
}
