// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using OneOf;

namespace DSharpPlus.Core.Models.Serialization.Converters;

/// <summary>
/// Provides a factory for OneOf converters.
/// </summary>
public sealed class OneOfConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType && typeToConvert.IsAssignableTo(typeof(IOneOf));

    public override JsonConverter? CreateConverter
    (
        Type typeToConvert, 
        JsonSerializerOptions options
    )
    {
        Type concreteConverter = typeof(OneOfConverter<>).MakeGenericType(typeToConvert);

        return (JsonConverter)Activator.CreateInstance(concreteConverter)!;
    }
}
