using System;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Logging;
using DSharpPlus.Metrics;
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
    private IPayloadDecompressor decompressor;
    private readonly ILoggerFactory factory;
    private readonly GatewayMetricsContainer metrics;
    private readonly TimeSpan sendingTimeout;

    private bool isConnected = false;
    private bool isDisposed = false;

    public TransportService
    (
        ILoggerFactory factory,
        IOptions<GatewayClientOptions> options,
        GatewayMetricsContainer metrics
    )
    {
        this.factory = factory;
        this.writer = new();
        this.decompressedWriter = new();
        this.metrics = metrics;

        this.sendingTimeout = options.Value.SendingTimeout;

        this.logger = factory.CreateLogger("DSharpPlus.Net.Gateway.ITransportService - invalid shard");
    }

    /// <inheritdoc/>
    public void Initialize(string loggerName, IPayloadDecompressor decompressor)
    {
        this.logger = this.factory.CreateLogger(loggerName);
        this.decompressor = decompressor;
    }

    /// <inheritdoc/>
    public async ValueTask ConnectAsync(string url)
    {
        this.socket = new();

        this.socket.Options.KeepAliveTimeout = this.sendingTimeout;

        this.decompressor.Initialize();

        ObjectDisposedException.ThrowIf(this.isDisposed, this);

        this.logger.LogTrace("Connecting to the Discord gateway.");

        await this.socket.ConnectAsync(new(url), CancellationToken.None);
        this.isConnected = true;

        this.logger.LogDebug("Connected to the Discord gateway at {url}, using {compression} compression.", url, this.decompressor.Name);
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
                        null,
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
    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public async ValueTask<TransportFrame> ReadAsync()
    {
        ObjectDisposedException.ThrowIf(this.isDisposed, this);

        if (!this.isConnected)
        {
            throw new InvalidOperationException("The transport service was not connected to the gateway.");
        }

        ValueWebSocketReceiveResult receiveResult = default;

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
            return new(ex, WebSocketMessageType.Close);
        }

        this.metrics.RecordGatewayEventReceived(this.writer.WrittenCount);

        if (!this.decompressor.TryDecompress(this.writer.WrittenSpan, this.decompressedWriter))
        {
            throw new InvalidDataException("Failed to decompress a gateway payload.");
        }

        this.metrics.RecordGatewayEventDecompressed(this.decompressedWriter.WrittenCount);

        if (this.logger.IsEnabled(LogLevel.Trace) && RuntimeFeatures.EnableInboundGatewayLogging)
        {
            if (receiveResult.MessageType == WebSocketMessageType.Text)
            {
                string result = Encoding.UTF8.GetString(this.decompressedWriter.WrittenSpan);

                string anonymized = result;

                if (RuntimeFeatures.AnonymizeTokens)
                {
                    anonymized = AnonymizationUtilities.AnonymizeTokens(anonymized);
                }

                if (RuntimeFeatures.AnonymizeContents)
                {
                    anonymized = AnonymizationUtilities.AnonymizeContents(anonymized);
                }

                this.logger.LogTrace("Received inbound plaintext gateway event (length: {length}): {event}", this.decompressedWriter.WrittenCount, anonymized);
            }
            else if (receiveResult.MessageType == WebSocketMessageType.Binary)
            {
                this.logger.LogTrace("Received inbound binary gateway event with length {length}", this.decompressedWriter.WrittenCount);
            }
        }

        return receiveResult.MessageType is WebSocketMessageType.Text or WebSocketMessageType.Binary
            ? new(this.decompressedWriter.WrittenSpan.ToArray(), receiveResult.MessageType)
            : new(this.socket.CloseStatus!, WebSocketMessageType.Close);
    }

    /// <inheritdoc/>
    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask WriteAsync(ReadOnlyMemory<byte> payload, WebSocketMessageType type)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(payload.Length, 4096, nameof(payload));

        if (this.logger.IsEnabled(LogLevel.Trace) && RuntimeFeatures.EnableOutboundGatewayLogging)
        {
            if (type == WebSocketMessageType.Text)
            {
                string anonymized = Encoding.UTF8.GetString(payload.Span);

                if (RuntimeFeatures.AnonymizeTokens)
                {
                    anonymized = AnonymizationUtilities.AnonymizeTokens(anonymized);
                }

                if (RuntimeFeatures.AnonymizeContents)
                {
                    anonymized = AnonymizationUtilities.AnonymizeContents(anonymized);
                }

                this.logger.LogTrace("Payload for the last outbound gateway event (length: {length}): {event}", payload.Length, anonymized);
            }
        }

        this.metrics.RecordGatewayEventSent(payload.Length);

        CancellationTokenSource source = new(this.sendingTimeout);

        // note: we want to always make this call, even if we could predict it's going to fail, because we need to know how it'll fail
        await this.socket.SendAsync
        (
            buffer: payload,
            messageType: type,
            endOfMessage: true,
            cancellationToken: source.Token
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
