// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

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

            this.jsonOptions.TypeInfoResolverChain.Insert
            (
                index: 0,
                item: ConstructTypeInfoResolver(formats.Value)
            );
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

    private static DefaultJsonTypeInfoResolver ConstructTypeInfoResolver
    (
        SerializationOptions models
    )
    {
        DefaultJsonTypeInfoResolver resolver = new();

        resolver.WithAddedModifier
        (
            (typeinfo) =>
            {
                if (models.InterfacesToConcrete.TryGetValue(typeinfo.Type, out Type? value))
                {
                    typeinfo.CreateObject = CreateObjectFactory(value);
                }
            }
        );

        return resolver;
    }

    // for future reference, we might want to optimize the false branch. it's never hit for our own models,
    // but it might hit for user-defined models and if there's a faster way of doing this we shouldn't penalize
    // them
    private static Func<object> CreateObjectFactory
    (
        Type type
    )
    {
        ConstructorInfo? ctor = type.GetConstructor(Type.EmptyTypes);

        if (ctor is not null)
        {
            DynamicMethod method = new
            (
                name: $"factory-{type.Name}",
                returnType: type,
                parameterTypes: Type.EmptyTypes,
                restrictedSkipVisibility: true
            );

            ILGenerator il = method.GetILGenerator();

            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Ret);

            return method.CreateDelegate<Func<object>>();
        }
        else
        {
            return () => RuntimeHelpers.GetUninitializedObject(type);
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
