using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice.Transport;

public class DiscordTransportService : IDiscordTransportService
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
        this.transportService = new TransportService(uri, OnBaseText, OnBaseBinary, configureOptions);
        this.jsonHandlers = jsonHandlers;
        this.binaryHandlers = binaryHandlers;
        this.logger = logger;
    }

    public async Task ConnectAsync() => await this.transportService.ConnectAsync();

    public async Task OnBaseText(string messageText)
    {
        BaseDiscordMessage? message = JsonSerializer.Deserialize<BaseDiscordMessage>(messageText);
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

    public async Task OnBaseBinary(ReadOnlyMemory<byte> binaryResponse)
    {
        List<Task> handlerTasks = [];
        if (binaryResponse.Length < 3)
        {
            // log invalid binary message was received
            this.logger.LogWarning("Invalid binary message was received!");

            return;
        }

        int opCode = BitConverter.ToInt32(binaryResponse.Span.Slice(2, 4));
        if (this.binaryHandlers.TryGetValue(opCode, out List<Func<ReadOnlyMemory<byte>, DiscordTransportService, Task>>? handler))
        {
            handlerTasks = [.. handler.Select(async (x, y) => await x.Invoke(binaryResponse, this))];
        }

        await Task.WhenAll(handlerTasks);
    }

    public async Task Send(ReadOnlyMemory<byte> data, CancellationToken? token = null) => await this.transportService.SendAsync(data, token);
    public async Task Send<T>(T data, CancellationToken? token = null) => await this.transportService.SendAsync(data, token);
}

public class DiscordTransportServiceBuilder : IDiscordTransportServiceBuilder
{
    private readonly ILogger<DiscordTransportService> logger;
    private readonly ConcurrentDictionary<int, List<Func<string, DiscordTransportService, Task>>> jsonHandlers = [];
    private readonly ConcurrentDictionary<int, List<Func<ReadOnlyMemory<byte>, DiscordTransportService, Task>>> binaryHandlers = [];
    private Action<ClientWebSocketOptions>? configureOptions = null;

    public DiscordTransportServiceBuilder(ILogger<DiscordTransportService> logger) => this.logger = logger;

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

    public void AddBinaryHandler(int opCode, Func<ReadOnlyMemory<byte>, DiscordTransportService, Task> handler)
    {
        if (!this.binaryHandlers.TryGetValue(opCode, out List<Func<ReadOnlyMemory<byte>, DiscordTransportService, Task>>? handlerTasks))
        {
            handlerTasks = [];
        }

        handlerTasks.Add(handler);

        this.binaryHandlers[opCode] = handlerTasks;
    }

    public void ConfigureWebSocketOptions(Action<ClientWebSocketOptions> configureOptions) => this.configureOptions = configureOptions;

    public DiscordTransportService Build(Uri uri)
    {
        if (this.jsonHandlers.IsEmpty && this.binaryHandlers.IsEmpty)
        {
            this.logger.LogWarning("Discord Transport Service was built with no handlers! No data will be handled.");
        }

        return new DiscordTransportService(uri, this.jsonHandlers, this.binaryHandlers, this.logger, this.configureOptions);
    }
}
