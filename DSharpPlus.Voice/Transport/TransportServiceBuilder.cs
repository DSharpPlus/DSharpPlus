using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice.Transport;

/// <summary>
/// Provides a convenience utility for registering payload handlers for a transport service.
/// </summary>
public sealed class TransportServiceBuilder : ITransportServiceBuilder, IInitializedTransportServiceBuilder
{
    private readonly ILogger<ITransportService> logger;
    private readonly ConcurrentDictionary<int, List<Func<string, TransportService, Task>>> jsonHandlers = [];
    private readonly ConcurrentDictionary<int, List<Func<ReadOnlyMemory<byte>, TransportService, Task>>> binaryHandlers = [];
    private Action<ClientWebSocketOptions>? configureOptions = null;

    public TransportServiceBuilder(ILogger<ITransportService> logger) 
        => this.logger = logger;

    /// <inheritdoc/>
    public void AddJsonHandler<T>(int opcode, Func<T, TransportService, Task> handler)
    {
        if (!this.jsonHandlers.TryGetValue(opcode, out List<Func<string, TransportService, Task>>? handlerTasks))
        {
            handlerTasks = [];
        }

        handlerTasks.Add(async (x, y) =>
        {
            T? deserializedObject = default;

            try
            {
                deserializedObject = JsonSerializer.Deserialize<T>(x);
            }
            catch
            {
            }

            if (deserializedObject is null)
            {
                this.logger.LogWarning("Json handler for opcode: {opCode} failed to deserialize to {type}", opcode, x.GetType().Name);
                return;
            }

            await handler.Invoke(deserializedObject, y);
        });

        this.jsonHandlers[opcode] = handlerTasks;
    }

    /// <inheritdoc/>
    public void AddBinaryHandler(int opcode, Func<ReadOnlyMemory<byte>, TransportService, Task> handler)
    {
        if (!this.binaryHandlers.TryGetValue(opcode, out List<Func<ReadOnlyMemory<byte>, TransportService, Task>>? handlerTasks))
        {
            handlerTasks = [];
        }

        handlerTasks.Add(async (data, client) => await handler.Invoke(data[3..], client) );

        this.binaryHandlers[opcode] = handlerTasks;
    }

    /// <inheritdoc/>
    public void ConfigureWebSocketOptions(Action<ClientWebSocketOptions> configureOptions) => this.configureOptions = configureOptions;

    /// <inheritdoc/>
    public ITransportService Build(Uri uri)
    {
        if (this.jsonHandlers.IsEmpty && this.binaryHandlers.IsEmpty)
        {
            throw new InvalidOperationException("The voice gateway transport service was not initialized correctly and no data can be handled.");
        }

        return new TransportService(uri, this.jsonHandlers, this.binaryHandlers, this.logger, this.configureOptions);
    }

    public IInitializedTransportServiceBuilder CreateBuilder() => new TransportServiceBuilder(this.logger);
}
