using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Logging;
using DSharpPlus.Net.Gateway.Compression;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Net.Gateway;

/// <inheritdoc cref="ITransportService"/>
internal sealed class TransportService : ITransportService
{
    private ILogger logger;
    private ClientWebSocket socket;
    private readonly ArrayPoolBufferWriter<byte> writer;
    private readonly ArrayPoolBufferWriter<byte> decompressedWriter;
    private readonly IPayloadDecompressor decompressor;
    private readonly ILoggerFactory factory;

    private readonly bool streamingDeserialization;

    private bool isConnected = false;
    private bool isDisposed = false;

    public TransportService
    (
        ILoggerFactory factory,
        IPayloadDecompressor decompressor,
        IOptions<GatewayClientOptions> options
    )
    {
        this.factory = factory;
        this.writer = new();
        this.decompressedWriter = new();
        this.decompressor = decompressor;

        this.streamingDeserialization = options.Value.EnableStreamingDeserialization;

        this.logger = factory.CreateLogger("DSharpPlus.Net.Gateway.ITransportService - invalid shard");
    }

    /// <inheritdoc/>
    public async ValueTask ConnectAsync(string url, int? shardId)
    {
        this.logger = shardId is null
            ? this.factory.CreateLogger("DSharpPlus.Net.Gateway.ITransportService")
            : this.factory.CreateLogger($"DSharpPlus.Net.Gateway.ITransportService - Shard {shardId}");

        this.socket = new();
        this.decompressor.Initialize();

        ObjectDisposedException.ThrowIf(this.isDisposed, this);

        if (this.isConnected)
        {
            this.logger.LogWarning("Attempted to connect, but there already is a connection opened. Ignoring.");
            return;
        }

        this.logger.LogTrace("Connecting to the Discord gateway.");

        await this.socket.ConnectAsync(new(url), CancellationToken.None);
        this.isConnected = true;

        this.logger.LogDebug("Connected to the Discord websocket, using {compression} compression.", this.decompressor.Name);
    }

    /// <inheritdoc/>
    public async ValueTask DisconnectAsync(WebSocketCloseStatus closeStatus)
    {
        this.logger.LogTrace("Disconnect requested: {CloseStatus}", closeStatus.ToString());

        if (!this.isConnected)
        {
            this.logger.LogTrace
            (
                "Attempting to disconnect from the Discord gateway, but there was no open connection. Ignoring."
            );

            return;
        }

        this.isConnected = false;

        switch (this.socket.State)
        {
            case WebSocketState.CloseSent:
            case WebSocketState.CloseReceived:
            case WebSocketState.Closed:
            case WebSocketState.Aborted:

                this.logger.LogTrace
                (
                    "Attempting to disconnect from the Discord gateway, but there is a disconnect in progress or complete. " +
                    "Current websocket state: {state}",
                    this.socket.State.ToString()
                );

                return;

            case WebSocketState.Open:
            case WebSocketState.Connecting:

                this.logger.LogTrace("Disconnecting. Current websocket state: {state}", this.socket.State.ToString());

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

        this.decompressor.Reset();
        this.socket.Dispose();
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

        if (this.logger.IsEnabled(LogLevel.Trace) && RuntimeFeatures.EnableInboundGatewayLogging)
        {
            string result = Encoding.UTF8.GetString(this.decompressedWriter.WrittenSpan);

            this.logger.LogTrace
            (
                "Length for the last inbound gateway event: {length}",
                this.writer.WrittenCount != 0 ? this.writer.WrittenCount : $"closed: {(int)this.socket.CloseStatus!}"
            );

            string anonymized = result;

            if (RuntimeFeatures.AnonymizeTokens)
            {
                anonymized = AnonymizationUtilities.AnonymizeTokens(anonymized);
            }

            if (RuntimeFeatures.AnonymizeContents)
            {
                anonymized = AnonymizationUtilities.AnonymizeContents(anonymized);
            }

            this.logger.LogTrace("Payload for the last inbound gateway event: {event}", anonymized);

            return this.writer.WrittenCount == 0 ? new((int)this.socket.CloseStatus!) : new(result);
        }
        else if (this.streamingDeserialization)
        {
            MemoryStream result = new(this.decompressedWriter.WrittenSpan.ToArray());
            return this.writer.WrittenCount == 0 ? new((int)this.socket.CloseStatus!) : new(result);
        }
        else
        {
            string result = Encoding.UTF8.GetString(this.decompressedWriter.WrittenSpan);
            return this.writer.WrittenCount == 0 ? new((int)this.socket.CloseStatus!) : new(result);
        }
    }

    /// <inheritdoc/>
    public async ValueTask WriteAsync(byte[] payload)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(payload.Length, 4096, nameof(payload));

        if (this.logger.IsEnabled(LogLevel.Trace) && RuntimeFeatures.EnableOutboundGatewayLogging)
        {

            this.logger.LogTrace("Length for the last outbound outbound event: {length}", payload.Length);

            string anonymized = Encoding.UTF8.GetString(payload);

            if (RuntimeFeatures.AnonymizeTokens)
            {
                anonymized = AnonymizationUtilities.AnonymizeTokens(anonymized);
            }

            if (RuntimeFeatures.AnonymizeContents)
            {
                anonymized = AnonymizationUtilities.AnonymizeContents(anonymized);
            }

            this.logger.LogTrace("Payload for the last outbound gateway event: {event}", anonymized);
        }

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
