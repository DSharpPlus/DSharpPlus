// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DSharpPlus.Serialization;

internal sealed class RedirectingConverter<TInterface, TModel> : JsonConverter<TInterface>
    where TModel : TInterface
{
    public override TInterface? Read
    (
        ref Utf8JsonReader reader, 
        Type typeToConvert, 
        JsonSerializerOptions options
    )
    {
        if (!options.TryGetTypeInfo(typeof(TModel), out JsonTypeInfo? typeInfo))
        {
            typeInfo = options.GetTypeInfo(typeof(TInterface));
        }

        return (TInterface?)JsonSerializer.Deserialize(ref reader, typeInfo);
    }

    public override void Write
    (
        Utf8JsonWriter writer, 
        TInterface value, 
        JsonSerializerOptions options
    )
    {
        if (!options.TryGetTypeInfo(value!.GetType(), out JsonTypeInfo? typeInfo))
        {
            typeInfo = options.GetTypeInfo(typeof(TInterface));
        }

        JsonSerializer.Serialize(writer, value, typeInfo);
    }
}
