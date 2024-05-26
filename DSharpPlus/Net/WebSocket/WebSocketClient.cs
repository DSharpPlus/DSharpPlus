using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.EventArgs;

using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.Net.WebSocket;

// weebsocket
// not even sure whether emzi or I posted this. much love, naam.
/// <summary>
/// The default, native-based WebSocket client implementation.
/// </summary>
public class WebSocketClient : IWebSocketClient
{
    private const int OutgoingChunkSize = 8192; // 8 KiB
    private const int IncomingChunkSize = 32768; // 32 KiB

    /// <inheritdoc />
    public IWebProxy Proxy { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> DefaultHeaders { get; }

    /// <inheritdoc />
    public bool IsConnected
        => this.isConnected;

    private readonly Dictionary<string, string> defaultHeaders;

    private Task receiverTask;
    private CancellationTokenSource receiverTokenSource;
    private CancellationToken receiverToken;
    private readonly SemaphoreSlim senderLock;

    private CancellationTokenSource socketTokenSource;
    private CancellationToken socketToken;
    private ClientWebSocket ws;

    private volatile bool isClientClose = false;
    private volatile bool isConnected = false;
    private bool isDisposed = false;

    /// <summary>
    /// Instantiates a new WebSocket client.
    /// </summary>
    private WebSocketClient(IClientErrorHandler handler)
    {
        this.connected = new(handler);
        this.disconnected = new(handler);
        this.messageReceived = new(handler);
        this.exceptionThrown = new(handler);

        this.defaultHeaders = [];
        this.DefaultHeaders = new ReadOnlyDictionary<string, string>(this.defaultHeaders);

        this.receiverTokenSource = null;
        this.receiverToken = CancellationToken.None;
        this.senderLock = new SemaphoreSlim(1);

        this.socketTokenSource = null;
        this.socketToken = CancellationToken.None;
    }

    /// <inheritdoc />
    public async Task ConnectAsync(Uri uri)
    {
        // Disconnect first
        try
        {
            await DisconnectAsync();
        }
        catch { }

        // Disallow sending messages
        await this.senderLock.WaitAsync();

        try
        {
            // This can be null at this point
            this.receiverTokenSource?.Dispose();
            this.socketTokenSource?.Dispose();

            this.ws?.Dispose();
            this.ws = new ClientWebSocket();
            this.ws.Options.Proxy = this.Proxy;
            this.ws.Options.KeepAliveInterval = TimeSpan.Zero;
            if (this.defaultHeaders != null)
            {
                foreach ((string k, string v) in this.defaultHeaders)
                {
                    this.ws.Options.SetRequestHeader(k, v);
                }
            }

            this.receiverTokenSource = new CancellationTokenSource();
            this.receiverToken = this.receiverTokenSource.Token;

            this.socketTokenSource = new CancellationTokenSource();
            this.socketToken = this.socketTokenSource.Token;

            this.isClientClose = false;
            this.isDisposed = false;
            await this.ws.ConnectAsync(uri, this.socketToken);
            this.receiverTask = Task.Run(ReceiverLoopAsync, this.receiverToken);
        }
        finally
        {
            this.senderLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task DisconnectAsync(int code = 1000, string message = "")
    {
        // Ensure that messages cannot be sent
        await this.senderLock.WaitAsync();

        try
        {
            this.isClientClose = true;
            if (this.ws != null && (this.ws.State == WebSocketState.Open || this.ws.State == WebSocketState.CloseReceived))
            {
                await this.ws.CloseOutputAsync((WebSocketCloseStatus)code, message, CancellationToken.None);
            }

            if (this.receiverTask != null)
            {
                await this.receiverTask; // Ensure that receiving completed
            }

            if (this.isConnected)
            {
                this.isConnected = false;
            }

            if (!this.isDisposed)
            {
                // Cancel all running tasks
                if (this.socketToken.CanBeCanceled)
                {
                    this.socketTokenSource?.Cancel();
                }

                this.socketTokenSource?.Dispose();

                if (this.receiverToken.CanBeCanceled)
                {
                    this.receiverTokenSource?.Cancel();
                }

                this.receiverTokenSource?.Dispose();

                this.isDisposed = true;
            }
        }
        catch { }
        finally
        {
            this.senderLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task SendMessageAsync(string message)
    {
        if (this.ws == null)
        {
            return;
        }

        if (this.ws.State is not WebSocketState.Open and not WebSocketState.CloseReceived)
        {
            return;
        }

        byte[] bytes = Utilities.UTF8.GetBytes(message);
        await this.senderLock.WaitAsync();
        try
        {
            int len = bytes.Length;
            int segCount = len / OutgoingChunkSize;
            if (len % OutgoingChunkSize != 0)
            {
                segCount++;
            }

            for (int i = 0; i < segCount; i++)
            {
                int segStart = OutgoingChunkSize * i;
                int segLen = Math.Min(OutgoingChunkSize, len - segStart);

                await this.ws.SendAsync(new ArraySegment<byte>(bytes, segStart, segLen), WebSocketMessageType.Text, i == segCount - 1, CancellationToken.None);
            }
        }
        finally
        {
            this.senderLock.Release();
        }
    }

    /// <inheritdoc />
    public bool AddDefaultHeader(string name, string value)
    {
        this.defaultHeaders[name] = value;
        return true;
    }

    /// <inheritdoc />
    public bool RemoveDefaultHeader(string name)
        => this.defaultHeaders.Remove(name);

    /// <summary>
    /// Disposes of resources used by this WebSocket client instance.
    /// </summary>
    //public void Dispose()
    //{
    //
    //}

    internal async Task ReceiverLoopAsync()
    {
        await Task.Yield();

        CancellationToken token = this.receiverToken;
        ArraySegment<byte> buffer = new(new byte[IncomingChunkSize]);

        try
        {
            using MemoryStream bs = new();
            while (!token.IsCancellationRequested)
            {
                // See https://github.com/RogueException/Discord.Net/commit/ac389f5f6823e3a720aedd81b7805adbdd78b66d
                // for explanation on the cancellation token

                WebSocketReceiveResult result;
                byte[] resultBytes;
                do
                {
                    result = await this.ws.ReceiveAsync(buffer, CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    bs.Write(buffer.Array, 0, result.Count);
                }
                while (!result.EndOfMessage);

                resultBytes = new byte[bs.Length];
                bs.Position = 0;
                bs.Read(resultBytes, 0, resultBytes.Length);
                bs.Position = 0;
                bs.SetLength(0);

                if (!this.isConnected && result.MessageType != WebSocketMessageType.Close)
                {
                    this.isConnected = true;
                    await this.connected.InvokeAsync(this, new SocketOpenedEventArgs());
                }

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    await this.messageReceived.InvokeAsync(this, new SocketBinaryMessageEventArgs(resultBytes));
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    await this.messageReceived.InvokeAsync(this, new SocketTextMessageEventArgs(Utilities.UTF8.GetString(resultBytes)));
                }
                else // close
                {
                    if (!this.isClientClose)
                    {
                        WebSocketCloseStatus code = result.CloseStatus.Value;
                        code = code is WebSocketCloseStatus.NormalClosure or WebSocketCloseStatus.EndpointUnavailable
                            ? (WebSocketCloseStatus)4000
                            : code;

                        await this.ws.CloseOutputAsync(code, result.CloseStatusDescription, CancellationToken.None);
                    }

                    await this.disconnected.InvokeAsync(this, new SocketClosedEventArgs() { CloseCode = (int)result.CloseStatus, CloseMessage = result.CloseStatusDescription });
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            await this.exceptionThrown.InvokeAsync(this, new SocketErrorEventArgs() { Exception = ex });
            await this.disconnected.InvokeAsync(this, new SocketClosedEventArgs() { CloseCode = -1, CloseMessage = "" });
        }

        // Don't await or you deadlock
        // DisconnectAsync waits for this method
        _ = DisconnectAsync();
    }

    #region Events
    /// <summary>
    /// Triggered when the client connects successfully.
    /// </summary>
    public event AsyncEventHandler<IWebSocketClient, SocketOpenedEventArgs> Connected
    {
        add => this.connected.Register(value);
        remove => this.connected.Unregister(value);
    }
    private readonly AsyncEvent<WebSocketClient, SocketOpenedEventArgs> connected;

    /// <summary>
    /// Triggered when the client is disconnected.
    /// </summary>
    public event AsyncEventHandler<IWebSocketClient, SocketClosedEventArgs> Disconnected
    {
        add => this.disconnected.Register(value);
        remove => this.disconnected.Unregister(value);
    }
    private readonly AsyncEvent<WebSocketClient, SocketClosedEventArgs> disconnected;

    /// <summary>
    /// Triggered when the client receives a message from the remote party.
    /// </summary>
    public event AsyncEventHandler<IWebSocketClient, SocketMessageEventArgs> MessageReceived
    {
        add => this.messageReceived.Register(value);
        remove => this.messageReceived.Unregister(value);
    }
    private readonly AsyncEvent<WebSocketClient, SocketMessageEventArgs> messageReceived;

    /// <summary>
    /// Triggered when an error occurs in the client.
    /// </summary>
    public event AsyncEventHandler<IWebSocketClient, SocketErrorEventArgs> ExceptionThrown
    {
        add => this.exceptionThrown.Register(value);
        remove => this.exceptionThrown.Unregister(value);
    }
    private readonly AsyncEvent<WebSocketClient, SocketErrorEventArgs> exceptionThrown;

    private void EventErrorHandler<TArgs>(AsyncEvent<WebSocketClient, TArgs> asyncEvent, Exception ex, AsyncEventHandler<WebSocketClient, TArgs> handler, WebSocketClient sender, TArgs eventArgs)
        where TArgs : AsyncEventArgs
        => this.exceptionThrown.InvokeAsync(this, new SocketErrorEventArgs() { Exception = ex }).GetAwaiter().GetResult();

    protected virtual void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            if (disposing)
            {
                DisconnectAsync().GetAwaiter().GetResult();
                this.receiverTokenSource?.Dispose();
                this.socketTokenSource?.Dispose();
            }

            this.isDisposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
