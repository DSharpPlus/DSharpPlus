using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Voice.Protocol.Gateway;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Voice.Transport;

/// <inheritdoc cref="ITransportService"/>
public sealed class TransportService : ITransportService
{
    private readonly ILoggerFactory loggerFactory;
    private readonly ArrayPoolBufferWriter<byte> receiveWriter;
    private readonly ArrayPoolBufferWriter<byte> sendWriter;
    private ILogger logger;
    private ClientWebSocket socket;

    private bool isDisposed;
    private bool isConnected;

    /// <inheritdoc/>
    public ushort SequenceNumber { get; internal set; }

    internal TransportService(ILoggerFactory loggerFactory)
    {
        this.receiveWriter = new();
        this.sendWriter = new();
        this.loggerFactory = loggerFactory;
    }

    /// <inheritdoc/>
    public async Task ConnectAsync(string url, ulong channelId)
    {
        ObjectDisposedException.ThrowIf(this.isDisposed, this);

        if (this.isConnected)
        {
            this.logger.LogWarning("Attempted to connect, but there already is a connection opened. Ignoring.");
            return;
        }

        this.logger = this.loggerFactory.CreateLogger($"DSharpPlus.Voice.Transport.ITransportService - Channel {channelId}");
        this.socket = new();

        this.logger.LogTrace("Connecting to the voice gateway.");

        await this.socket.ConnectAsync(new(url), CancellationToken.None);
        this.isConnected = true;

        this.logger.LogDebug("Connected to the voice gateway.");
    }

    /// <inheritdoc/>
    public async Task DisconnectAsync(WebSocketCloseStatus closeStatus)
    {
        this.logger.LogTrace("Disconnect requested: {CloseStatus}", closeStatus.ToString());

        if (!this.isConnected)
        {
            this.logger.LogTrace
            (
                "Attempting to disconnect from the voice gateway, but there was no open connection. Ignoring."
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
                    await this.socket.CloseAsync(closeStatus, "Disconnecting.", CancellationToken.None);
                }
                catch (WebSocketException) { }
                catch (OperationCanceledException) { }

                break;
        }

        this.socket.Dispose();
    }

    /// <inheritdoc/>
    public async Task SendBinaryAsync(ReadOnlyMemory<byte> payload)
    {
        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            this.logger.LogTrace("Sent outbound binary voice gateway event with opcode {opcode}", payload.Span[0]);
        }

        if (this.isConnected)
        {
            await this.socket.SendAsync
            (
                buffer: payload,
                messageType: WebSocketMessageType.Binary,
                endOfMessage: true,
                cancellationToken: CancellationToken.None
            );
        }
    }

    /// <inheritdoc/>
    public async Task SendTextAsync(VoiceGatewayMessage payload)
    {
        Utf8JsonWriter writer = new(this.sendWriter);
        JsonSerializer.Serialize(writer, payload);

        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            this.logger.LogTrace("Payload for the last outbound voice gateway event: {event}", Encoding.UTF8.GetString(this.sendWriter.WrittenSpan));
        }

        if (this.isConnected)
        {
            await this.socket.SendAsync
            (
                buffer: this.sendWriter.WrittenMemory,
                messageType: WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: CancellationToken.None
            );
        }
    }

    /// <inheritdoc/>
    public async Task<VoiceGatewayTransportFrame> ReceiveAsync()
    {
        ObjectDisposedException.ThrowIf(this.isDisposed, this);

        if (!this.isConnected)
        {
            throw new InvalidOperationException("The transport service was not connected to the voice gateway.");
        }

        ValueWebSocketReceiveResult receiveResult = default;

        this.receiveWriter.Clear();

        try
        {
            do
            {
                receiveResult = await this.socket.ReceiveAsync(this.receiveWriter.GetMemory(), CancellationToken.None);
                this.receiveWriter.Advance(receiveResult.Count);
            } while (!receiveResult.EndOfMessage);
        }
        catch (OperationCanceledException) { }

        if (receiveResult.MessageType == WebSocketMessageType.Binary)
        {
            int opcode = this.receiveWriter.WrittenSpan[2];
            byte[] payload = new byte[this.receiveWriter.WrittenCount];
            this.receiveWriter.WrittenSpan.CopyTo(payload);

            this.logger.LogTrace("Received binary event from the voice gateway: (opcode {opcode}, length {length})", opcode, this.receiveWriter.WrittenCount);

            return new VoiceGatewayTransportFrame
            {
                Opcode = (VoiceGatewayOpcode)opcode,
                Type = WebSocketMessageType.Binary,
                Payload = payload
            };
        }
        else if (receiveResult.MessageType == WebSocketMessageType.Text)
        {
            int index = this.receiveWriter.WrittenSpan.IndexOf("\"op\":"u8) + 5;
            int endIndex = this.receiveWriter.WrittenSpan[index..].IndexOfAny(" ,"u8);

            int opcode = int.Parse(this.receiveWriter.WrittenSpan[index..endIndex]);

            byte[] payload = new byte[this.receiveWriter.WrittenCount];
            this.receiveWriter.WrittenSpan.CopyTo(payload);

            if (this.logger.IsEnabled(LogLevel.Trace))
            {
                this.logger.LogTrace
                (
                    "Received event from the voice gateway (length: {length}):\n{event}",
                    this.receiveWriter.WrittenCount,
                    Encoding.UTF8.GetString(this.receiveWriter.WrittenSpan)
                );
            }

            return new VoiceGatewayTransportFrame
            {
                Opcode = (VoiceGatewayOpcode)opcode,
                Type = WebSocketMessageType.Text,
                Payload = payload
            };
        }
        else // WebSocketMessageType.Close
        {
            return new VoiceGatewayTransportFrame
            {
                Opcode = (VoiceGatewayOpcode)(-1),
                Type = WebSocketMessageType.Close,
                Payload = [],
                Error = (VoiceGatewayCloseCode)(int)this.socket.CloseStatus!
            };
        }
    }

    public void Dispose()
    {
        this.socket.Dispose();
        this.isConnected = false;
        this.isDisposed = true;
    }
}
