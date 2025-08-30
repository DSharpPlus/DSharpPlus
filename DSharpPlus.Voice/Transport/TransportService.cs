using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice.Transport;

/// <inheritdoc cref="ITransportService"/>
public sealed class TransportService : ITransportService
{
    private readonly ILogger<ITransportService> logger;
    private readonly ConcurrentDictionary<int, List<Func<string, TransportService, Task>>> jsonHandlers;
    private readonly ConcurrentDictionary<int, List<Func<ReadOnlyMemory<byte>, TransportService, Task>>> binaryHandlers;
    private readonly TransportServiceCore transportService;

    /// <inheritdoc/>
    public ushort SequenceNumber { get; internal set; }

    internal TransportService
    (
        Uri uri,
        ConcurrentDictionary<int, List<Func<string, TransportService, Task>>> jsonHandlers,
        ConcurrentDictionary<int, List<Func<ReadOnlyMemory<byte>, TransportService, Task>>> binaryHandlers,
        ILogger<ITransportService> logger,
        Action<ClientWebSocketOptions>? configureOptions = null
    )
    {
        this.transportService = new TransportServiceCore(uri, OnTextReceivedAsync, OnBinaryReceivedAsync, configureOptions);
        this.jsonHandlers = jsonHandlers;
        this.binaryHandlers = binaryHandlers;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task ConnectAsync(CancellationToken? cancellationToken = null) => await this.transportService.ConnectAsync(cancellationToken);

    /// <inheritdoc/>
    private async Task OnTextReceivedAsync(string messageText)
    {
        using JsonDocument doc = JsonDocument.Parse(messageText);
        List<Task> handlerTasks = [];

        if (doc.RootElement.TryGetProperty("s", out JsonElement property))
        {
            this.SequenceNumber = property.GetUInt16();
        }

        int opCode = doc.RootElement.GetProperty("op").GetUInt16();
        this.logger.LogDebug("Received JSON OpCode: {opcode}", opCode);

        if (this.jsonHandlers.TryGetValue(opCode, out List<Func<string, TransportService, Task>>? handler))
        {
            handlerTasks = [.. handler.Select(async x => await x.Invoke(messageText, this))];
        }

        await Task.WhenAll(handlerTasks);
    }

    /// <inheritdoc/>
    private async Task OnBinaryReceivedAsync(ReadOnlyMemory<byte> binaryResponse)
    {
        List<Task> handlerTasks = [];

        if (binaryResponse.Length < 3)
        {
            // log invalid binary message was received
            this.logger.LogDebug("Invalid binary message was received!");

            return;
        }

        this.SequenceNumber = BinaryPrimitives.ReadUInt16BigEndian(binaryResponse.Span);
        int opCode = binaryResponse.Span[2];
        this.logger.LogDebug("Received Binary OpCode: {opcode}", opCode);

        if (this.binaryHandlers.TryGetValue(opCode, out List<Func<ReadOnlyMemory<byte>, TransportService, Task>>? handler))
        {
            handlerTasks = [.. handler.Select(async (x, y) => await x.Invoke(binaryResponse, this))];
        }

        await Task.WhenAll(handlerTasks);
    }

    /// <inheritdoc/>
    public async Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken? token = null)
    {
        this.logger.LogDebug("Sending Binary with OpCode: {opCode}", (int)data.Span[0]);
        await this.transportService.SendAsync(data, token);
    }

    /// <inheritdoc/>
    public async Task SendAsync<T>(T data, CancellationToken? token = null) where T : class => await this.transportService.SendAsync(data, token);
    public void Dispose() => this.transportService?.Dispose();
}
