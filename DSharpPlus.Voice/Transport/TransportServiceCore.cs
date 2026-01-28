using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Voice.Transport;

/// <summary>
/// This service creates a websocket connection and has a callback for binary and text received.
/// It also allows for sending messages to the remote connection.
/// </summary>
internal sealed class TransportServiceCore : IDisposable
{
    private readonly Uri uri;
    private readonly Func<string, Task> onTextAsync;
    private readonly Func<ReadOnlyMemory<byte>, Task> onBinaryAsync;
    private readonly ClientWebSocket webSocketClient;
    private readonly SemaphoreSlim receiveSemaphore = new(1);
    private readonly SemaphoreSlim sendSemaphore = new(1);
    private const int ReceiveLoopTimeout = 5000;

    internal TransportServiceCore
    (
        Uri uri,
        Func<string, Task> onTextAsync,
        Func<ReadOnlyMemory<byte>, Task> onBinaryAsync,
        Action<ClientWebSocketOptions>? configureOptions = null
    )
    {
        this.onTextAsync = onTextAsync;
        this.onBinaryAsync = onBinaryAsync;
        this.uri = uri;
        this.webSocketClient = new();

        configureOptions?.Invoke(this.webSocketClient.Options);
    }

    /// <summary>
    /// Starts the connection to the remote uri.
    /// Also starts the loop for receiving messages from remote to our local.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token passed into the internal websocket client</param>
    /// <returns></returns>
    public async Task ConnectAsync(CancellationToken? cancellationToken = null)
    {
        await this.webSocketClient.ConnectAsync(this.uri, cancellationToken ?? CancellationToken.None);

        // Start the receive loop
        _ = Task.Run(async () => await ReceiveLoopAsync());
    }

    /// <summary>
    /// Sends a data frame to the configured remote uri.
    /// </summary>
    /// <param name="data">data frame</param>
    /// <param name="token">Cancelation token passed into the internal websocket client for this messsage</param>
    /// <returns></returns>
    public async Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken? token = null)
    {
        await this.sendSemaphore.WaitAsync();
        token ??= CancellationToken.None;

        try
        {
            await this.webSocketClient.SendAsync(data, WebSocketMessageType.Binary, true, token.Value);
        }
        finally
        {
            this.sendSemaphore.Release();
        }
    }

    /// <summary>
    /// Sends JSON data to the remote uri.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">data frame to send to remote uri</param>
    /// <param name="token">Cancellation token passed into internal websocket client for this message</param>
    /// <returns></returns>
    public async Task SendAsync<T>(T data, CancellationToken? token = null)
    {
        await this.sendSemaphore.WaitAsync();
        token ??= CancellationToken.None;

        string jsonString = JsonSerializer.Serialize(data);
        byte[] jsonBinary = Encoding.UTF8.GetBytes(jsonString);

        try
        {
            await this.webSocketClient.SendAsync(jsonBinary, WebSocketMessageType.Text, true, token.Value);
        }
        finally
        {
            this.sendSemaphore.Release();
        }
    }

    /// <summary>
    /// This method waits for messages from the remote uri to local.
    /// When a message is received then we call our callback method.
    /// </summary>
    /// <param name="token">Cancellation token passed into internal websocket client</param>
    /// <returns></returns>
    private async Task ReceiveLoopAsync(CancellationToken? token = null)
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(8192);
        token ??= CancellationToken.None;

        try
        {
            ArrayPoolBufferWriter<byte> writer = new();
            while (!token.Value.IsCancellationRequested)
            {
                await this.receiveSemaphore.WaitAsync(token.Value);

                try
                {
                    if (this.webSocketClient is null || this.webSocketClient.State != WebSocketState.Open)
                    {
                        break;
                    }

                    ValueWebSocketReceiveResult result;

                    try
                    {
                        result = await this.webSocketClient.ReceiveAsync(buffer.AsMemory(0, buffer.Length), token.Value);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await this.webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "ack", token.Value);
                        break;
                    }

                    writer.Write(buffer.AsSpan(0, result.Count));

                    if (result.EndOfMessage)
                    {
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            string text = Encoding.UTF8.GetString(writer.WrittenSpan);

                            await this.onTextAsync(text);
                        }
                        else if (result.MessageType == WebSocketMessageType.Binary)
                        {
                            await this.onBinaryAsync(writer.WrittenMemory);
                        }

                        writer.Clear();
                    }

                    if (result.Count == 0)
                    {
                        await Task.Delay(ReceiveLoopTimeout, token.Value);
                    }
                }
                finally
                {
                    this.receiveSemaphore.Release();
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.webSocketClient?.Dispose();
        this.sendSemaphore.Dispose();
        this.receiveSemaphore?.Dispose();
    }
}
