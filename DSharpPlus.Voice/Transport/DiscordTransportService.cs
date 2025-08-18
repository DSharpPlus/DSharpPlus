using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Voice.Transport.Models;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice.Transport;

public class DiscordTransportService : ITransportService
{
    private readonly ILogger<DiscordTransportService> logger;
    private readonly ConcurrentDictionary<int, List<Func<string, DiscordTransportService, Task>>> jsonHandlers;
    private readonly ConcurrentDictionary<int, List<Func<ReadOnlyMemory<byte>, DiscordTransportService, Task>>> binaryHandlers;
    private readonly TransportService transportService;

    internal DiscordTransportService(Uri uri,
        ConcurrentDictionary<int, List<Func<string, DiscordTransportService, Task>>> jsonHandlers,
        ConcurrentDictionary<int, List<Func<ReadOnlyMemory<byte>, DiscordTransportService, Task>>> binaryHandlers,
        ILogger<DiscordTransportService> logger,
        Action<ClientWebSocketOptions>? configureOptions = null)
    {
        this.transportService = new TransportService(uri, OnBaseTextAsync, OnBaseBinaryAsync, configureOptions);
        this.jsonHandlers = jsonHandlers;
        this.binaryHandlers = binaryHandlers;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task ConnectAsync(CancellationToken? cancellationToken = null) => await this.transportService.ConnectAsync(cancellationToken);

    /// <inheritdoc/>
    private async Task OnBaseTextAsync(string messageText)
    {
        BaseDiscordGatewayMessage? message = JsonSerializer.Deserialize<BaseDiscordGatewayMessage>(messageText);
        List<Task> handlerTasks = [];

        if (message == null)
        {
            this.logger.LogWarning("Invalid string message was received!");
            this.logger.LogDebug("Invalid string message was received: {messageText}", messageText);

            return;
        }

        if (this.jsonHandlers.TryGetValue(message.OpCode, out List<Func<string, DiscordTransportService, Task>>? handler))
        {
            handlerTasks = [.. handler.Select(async x => await x.Invoke(messageText, this))];
        }

        await Task.WhenAll(handlerTasks);
    }

    /// <inheritdoc/>
    private async Task OnBaseBinaryAsync(ReadOnlyMemory<byte> binaryResponse)
    {
        List<Task> handlerTasks = [];
        if (binaryResponse.Length < 3)
        {
            // log invalid binary message was received
            this.logger.LogWarning("Invalid binary message was received!");

            return;
        }

        int opCode = binaryResponse.Span[2];
        if (this.binaryHandlers.TryGetValue(opCode, out List<Func<ReadOnlyMemory<byte>, DiscordTransportService, Task>>? handler))
        {
            handlerTasks = [.. handler.Select(async (x, y) => await x.Invoke(binaryResponse, this))];
        }

        await Task.WhenAll(handlerTasks);
    }

    /// <inheritdoc/>
    public async Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken? token = null) => await this.transportService.SendAsync(data, token);
    /// <inheritdoc/>
    public async Task SendAsync<T>(T data, CancellationToken? token = null) => await this.transportService.SendAsync(data, token);
}

public class DiscordTransportServiceBuilder : IDiscordTransportServiceBuilder
{
    private readonly ILogger<DiscordTransportService> logger;
    private readonly ConcurrentDictionary<int, List<Func<string, DiscordTransportService, Task>>> jsonHandlers = [];
    private readonly ConcurrentDictionary<int, List<Func<ReadOnlyMemory<byte>, DiscordTransportService, Task>>> binaryHandlers = [];
    private Action<ClientWebSocketOptions>? configureOptions = null;

    public DiscordTransportServiceBuilder(ILogger<DiscordTransportService> logger) => this.logger = logger;

    /// <inheritdoc/>
    public void AddJsonHandler<T>(int opCode, Func<T, DiscordTransportService, Task> handler)
    {
        if (!this.jsonHandlers.TryGetValue(opCode, out List<Func<string, DiscordTransportService, Task>>? handlerTasks))
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
                this.logger.LogWarning("Json handler for OpCode: {opCode} failed to deserialize to {type}", opCode, x.GetType().Name);
                return;
            }

            await handler.Invoke(deserializedObject, y);
        });

        this.jsonHandlers[opCode] = handlerTasks;
    }

    /// <inheritdoc/>
    public void AddBinaryHandler(int opCode, Func<ReadOnlyMemory<byte>, DiscordTransportService, Task> handler)
    {
        if (!this.binaryHandlers.TryGetValue(opCode, out List<Func<ReadOnlyMemory<byte>, DiscordTransportService, Task>>? handlerTasks))
        {
            handlerTasks = [];
        }

        handlerTasks.Add(handler);

        this.binaryHandlers[opCode] = handlerTasks;
    }

    /// <inheritdoc/>
    public void ConfigureWebSocketOptions(Action<ClientWebSocketOptions> configureOptions) => this.configureOptions = configureOptions;

    /// <inheritdoc/>
    public ITransportService Build(Uri uri)
    {
        if (this.jsonHandlers.IsEmpty && this.binaryHandlers.IsEmpty)
        {
            this.logger.LogWarning("Discord Transport Service was built with no handlers! No data will be handled.");
        }

        return new DiscordTransportService(uri, this.jsonHandlers, this.binaryHandlers, this.logger, this.configureOptions);
    }
}
