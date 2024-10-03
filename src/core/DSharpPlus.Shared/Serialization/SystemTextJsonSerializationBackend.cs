// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

using CommunityToolkit.HighPerformance.Buffers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Serialization;

/// <summary>
/// Implements a System.Text.Json-based serialization backend; the default backend used across DSharpPlus.
/// </summary>
public sealed class SystemTextJsonSerializationBackend : ISerializationBackend
{
    private readonly JsonSerializerOptions? jsonOptions;

    public SystemTextJsonSerializationBackend
    (
        IOptions<SerializationOptions> formats,
        IServiceProvider provider
    )
    {
        this.jsonOptions = provider.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().Get("dsharpplus");

        foreach (KeyValuePair<Type, Type> map in formats.Value.InterfacesToConcrete)
        {
            this.jsonOptions.Converters.Add
            (
                (JsonConverter)typeof(RedirectingConverter<,>)
                    .MakeGenericType(map.Key, map.Value)
                    .GetConstructor(Type.EmptyTypes)!
                    .Invoke(null)!
            );
        }

        this.jsonOptions.MakeReadOnly();
    }

    // the default STJ backend gets to be just 'json'
    public static string Id => "json";

    /// <inheritdoc/>
    public TModel DeserializeModel<TModel>(ReadOnlySpan<byte> data)
        where TModel : notnull
        => JsonSerializer.Deserialize<TModel>(data, this.jsonOptions)!;

    /// <inheritdoc/>
    public void SerializeModel<TModel>(TModel model, ArrayPoolBufferWriter<byte> target)
        where TModel : notnull
    {
        using Utf8JsonWriter writer = new(target);
        JsonSerializer.Serialize(writer, model, this.jsonOptions!);
    }
}
