using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Net.WebSocket;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Net.Gateway;

/// <inheritdoc cref="ITransportService"/>
internal sealed class TransportService : ITransportService
{
    private ILogger logger;
    private readonly ClientWebSocket socket;
    private readonly ArrayPoolBufferWriter<byte> writer;
    private readonly ArrayPoolBufferWriter<byte> decompressedWriter;
    private readonly PayloadDecompressor decompressor;
    private readonly ILoggerFactory factory;

    private bool isConnected = false;
    private bool isDisposed = false;

    public TransportService(ILoggerFactory factory, PayloadDecompressor decompressor)
    {
        this.factory = factory;
        this.socket = new();
        this.writer = new();
        this.decompressedWriter = new();
        this.decompressor = decompressor;

        this.logger = factory.CreateLogger("DSharpPlus.Net.Gateway.ITransportService - invalid shard");
    }

    /// <inheritdoc/>
    public async ValueTask ConnectAsync(string url, int? shardId)
    {
        this.logger = shardId is null
            ? this.factory.CreateLogger("DSharpPlus.Net.Gateway.ITransportService")
            : this.factory.CreateLogger($"DSharpPlus.Net.Gateway.ITransportService - Shard {shardId}");

        ObjectDisposedException.ThrowIf(this.isDisposed, this);

        if (this.isConnected)
        {
            this.logger.LogWarning("Attempted to connect, but there already is a connection opened. Ignoring.");
            return;
        }

        this.logger.LogTrace("Connecting to the Discord gateway.");

        await this.socket.ConnectAsync(new($"{url}?v=10&encoding=json"), CancellationToken.None);
        this.isConnected = true;

        this.logger.LogTrace("Connected to the Discord websocket.");
    }

    /// <inheritdoc/>
    public async ValueTask DisconnectAsync(WebSocketCloseStatus closeStatus)
    {
        this.logger.LogTrace("Disconnect requested: {CloseStatus}", closeStatus.ToString());

        if (!this.isConnected)
        {
            this.logger.LogWarning
            (
                "Attempting to disconnect from the Discord gateway, but there was no open connection. Ignoring."
            );
        }

        this.isConnected = false;

        switch (this.socket.State)
        {
            case WebSocketState.CloseSent:
            case WebSocketState.CloseReceived:
            case WebSocketState.Closed:
            case WebSocketState.Aborted:

                this.logger.LogWarning
                (
                    "Attempting to disconnect from the Discord gateway, but there is a disconnect in progress or complete. " +
                    "Current websocket state: {state}",
                    this.socket.State.ToString()
                );

                return;

            case WebSocketState.Open:
            case WebSocketState.Connecting:

                this.logger.LogDebug("Disconnecting. Current websocket state: {state}", this.socket.State.ToString());

                try
                {
                    await this.socket.CloseAsync
                    (
                        closeStatus,
                        "Disconnecting.",
                        CancellationToken.None
                    );
                }
                catch (WebSocketException) { }
                catch (OperationCanceledException) { }

                break;
        }
    }

    /// <inheritdoc/>
    public async ValueTask<TransportFrame> ReadAsync()
    {
        ObjectDisposedException.ThrowIf(this.isDisposed, this);

        if (!this.isConnected)
        {
            throw new InvalidOperationException("The transport service was not connected to the gateway.");
        }

        ValueWebSocketReceiveResult receiveResult;

        this.writer.Clear();
        this.decompressedWriter.Clear();

        try
        {
            do
            {
                receiveResult = await this.socket.ReceiveAsync(this.writer.GetMemory(), CancellationToken.None);

                this.writer.Advance(receiveResult.Count);

            } while (!receiveResult.EndOfMessage);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            return new(ex);
        }

        if (!this.decompressor.TryDecompress(this.writer.WrittenSpan, this.decompressedWriter))
        {
            throw new InvalidDataException("Failed to decompress a gateway payload.");
        }

        string result = Encoding.UTF8.GetString(this.decompressedWriter.WrittenSpan);

#if DEBUG
        this.logger.LogTrace("Length for the last inbound gateway event: {length}", this.writer.WrittenCount);
        this.logger.LogTrace("Payload for the last inbound gateway event:\n{event}", result);

        if (this.writer.WrittenCount == 0)
        {
            this.logger.LogTrace("Disconnected: {CloseStatus}", this.socket.CloseStatus.Value.ToString());
        }
#endif

        return this.writer.WrittenCount == 0 ? new((int)this.socket.CloseStatus!) : new(result);
    }

    /// <inheritdoc/>
    public async ValueTask WriteAsync(byte[] payload)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(payload.Length, 4096, nameof(payload));

#if DEBUG
        this.logger.LogTrace
        (
            "Sending outbound gateway event:\n{event}",
            Encoding.UTF8.GetString(payload)
        );
#endif
        await this.socket.SendAsync
        (
            buffer: payload,
            messageType: WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: CancellationToken.None
        );
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!this.isDisposed)
        {
            this.socket.Dispose();
        }

        this.isDisposed = true;
        GC.SuppressFinalize(this);
    }
}
