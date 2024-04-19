// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

using CommunityToolkit.HighPerformance.Buffers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Serialization;

/// <summary>
/// Handles serializing and deserializing models between different formats as necessary.
/// </summary>
/// <typeparam name="T">The library component a given instance is associated with.</typeparam>
[RequiresDynamicCode("Serialization in DSharpPlus presently depends on unreferenced and dynamic code.")]
[RequiresUnreferencedCode("Serialization in DSharpPlus presently depends on unreferenced and dynamic code.")]
public sealed partial class SerializationService<T> : ISerializationService<T>
{
    private readonly string format;

    private readonly JsonSerializerOptions? jsonOptions;

    public SerializationService
    (
        IOptions<SerializationOptions> formats,
        ILogger<ISerializationService<T>> logger,
        IServiceProvider provider
    )
    {
        if (formats.Value.Formats.TryGetValue(typeof(T), out string? format))
        {
            LogFormatSpecified(logger, format, typeof(T));

            this.format = format;
        }
        else
        {
            LogFormatFallback(logger, typeof(T));

            this.format = "json";
        }

        if (this.format == "json")
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
    }

    /// <inheritdoc/>
    public TModel DeserializeModel<TModel>
    (
        ReadOnlySpan<byte> data
    )
        where TModel : notnull
    {
        return this.format switch
        {
            "json" => JsonSerializer.Deserialize<TModel>(data, this.jsonOptions)!,
            _ => throw new InvalidOperationException($"The model could not be deserialized from {this.format}."),
        };
    }

    /// <inheritdoc/>
    public void SerializeModel<TModel>
    (
        TModel model,
        ArrayPoolBufferWriter<byte> target
    )
        where TModel : notnull
    {
        switch (this.format)
        {
            case "json":
                {
                    using Utf8JsonWriter writer = new(target);
                    JsonSerializer.Serialize(writer, model, this.jsonOptions!);
                    break;
                }
            default:
                throw new InvalidOperationException($"The model could not be serialized to {this.format}.");
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Setting serialization format {Format} for library component {Component}."
    )]
    private static partial void LogFormatSpecified
    (
        ILogger logger,
        string format,
        Type component
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "No serialization format for library component {Component} found, falling back to \"json\"."
    )]
    private static partial void LogFormatFallback
    (
        ILogger logger,
        Type component
    );
}
