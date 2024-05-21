// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DSharpPlus.Internal.Models.Serialization.Converters;

/// <summary>
/// A converter for <see cref="Optional{T}"/>.
/// </summary>
public sealed class OptionalConverter<T> : JsonConverter<Optional<T>>
{
    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(JsonSerializer.Deserialize<T>(ref reader, options)!);

    public override void Write
    (
        Utf8JsonWriter writer,
        Optional<T> value,
        JsonSerializerOptions options
    )
    {
        if (!value.HasValue)
        {
            throw new ArgumentException("Serializing an empty optional is not allowed.");
        }

        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
