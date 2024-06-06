using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance.Buffers;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Net.Gateway;

/// <inheritdoc/>
internal sealed class TransportService : ITransportService
{
    private readonly ILogger<ITransportService> logger;
    private readonly ClientWebSocket socket;
    private readonly ArrayPoolBufferWriter<byte> writer;

    private bool isConnected = false;
    private bool isDisposed = false;

    /// <summary>
    /// Specifies the URL this transport service will attempt to resume to.
    /// </summary>
    public string? ResumeUrl { get; private set; }

    public TransportService(ILogger<ITransportService> logger)
    {
        this.logger = logger;
        this.socket = new();
        this.writer = new();
    }

    /// <inheritdoc/>
    public async ValueTask ConnectAsync(string? url = null)
    {
        ObjectDisposedException.ThrowIf(this.isDisposed, this);

        if (this.isConnected)
        {
            this.logger.LogWarning("Attempted to connect, but there already is a connection opened. Ignoring.");
            return;
        }

        if (this.ResumeUrl is null)
        {
            this.logger.LogDebug("Connecting to the Discord gateway.");

            await this.socket.ConnectAsync(new($"{url}?v=10&encoding=json"), CancellationToken.None);
        }
        else
        {
            this.logger.LogDebug("Attempting to resume existing session.");

            await this.socket.ConnectAsync(new($"{this.ResumeUrl}?v=10&encoding=json"), CancellationToken.None);
        }

        this.isConnected = true;

        this.logger.LogDebug("Connected to the Discord websocket.");
    }

    /// <inheritdoc/>
    public async ValueTask DisconnectAsync(WebSocketCloseStatus closeStatus)
    {
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
    public async ValueTask<string> ReadAsync()
    {
        ObjectDisposedException.ThrowIf(this.isDisposed, this);

        if (!this.isConnected)
        {
            throw new InvalidOperationException("The transport service was not connected to the gateway.");
        }

        ValueWebSocketReceiveResult receiveResult;

        this.writer.Clear();

        try
        {
            do
            {
                receiveResult = await this.socket.ReceiveAsync(this.writer.GetMemory(), CancellationToken.None);

                this.writer.Advance(receiveResult.Count);

            } while (!receiveResult.EndOfMessage);
        }
        catch (OperationCanceledException) { }

        string result = Encoding.UTF8.GetString(this.writer.WrittenSpan);

#if DEBUG
        this.logger.LogTrace("Length for the last inbound gateway event: {length}", this.writer.WrittenCount);
        this.logger.LogTrace("Payload for the last inbound gateway event:\n{event}", result);
#endif

        return this.writer.WrittenCount == 0 ? string.Empty : result;
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
