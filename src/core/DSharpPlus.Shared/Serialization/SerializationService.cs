// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Serialization;

/// <summary>
/// Handles serializing and deserializing models between different formats as necessary.
/// </summary>
[RequiresDynamicCode("Serialization in DSharpPlus presently depends on unreferenced and dynamic code.")]
[RequiresUnreferencedCode("Serialization in DSharpPlus presently depends on unreferenced and dynamic code.")]
public sealed partial class SerializationService<T>
(
    IOptions<SerializationOptions> options,
    IServiceProvider services,
    ILogger<SerializationService<T>> logger
)
{
    /// <inheritdoc/>
    public TModel DeserializeModel<TModel>(ReadOnlySpan<byte> data)
        where TModel : notnull
    {
        ISerializationBackend backendImpl;

        if
        (
            options.Value.Formats.TryGetValue(typeof(T), out string? name)
            && options.Value.BackendImplementations.TryGetValue(name, out Type? backendType)
        )
        {
            backendImpl = (ISerializationBackend)services.GetRequiredService(backendType);
        }
        else
        {
            LogFormatFallback(logger, typeof(T));
            backendImpl = services.GetRequiredService<SystemTextJsonSerializationBackend>();
        }

        return backendImpl.DeserializeModel<TModel>(data);
    }

    /// <inheritdoc/>
    public void SerializeModel<TModel>
    (
        TModel model,
        ArrayPoolBufferWriter<byte> target
    )
        where TModel : notnull
    {
        ISerializationBackend backendImpl;

        if
        (
            options.Value.Formats.TryGetValue(typeof(T), out string? name)
            && options.Value.BackendImplementations.TryGetValue(name, out Type? backendType)
        )
        {
            backendImpl = (ISerializationBackend)services.GetRequiredService(backendType);
        }
        else
        {
            LogFormatFallback(logger, typeof(T));
            backendImpl = services.GetRequiredService<SystemTextJsonSerializationBackend>();
        }

        backendImpl.SerializeModel(model, target);
    }

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
