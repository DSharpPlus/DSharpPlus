// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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

namespace DSharpPlus.Net.WebSocket
{
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
        private readonly Dictionary<string, string> _defaultHeaders;

        private Task _receiverTask;
        private CancellationTokenSource _receiverTokenSource;
        private CancellationToken _receiverToken;
        private readonly SemaphoreSlim _senderLock;

        private CancellationTokenSource _socketTokenSource;
        private CancellationToken _socketToken;
        private ClientWebSocket _ws;

        private volatile bool _isClientClose = false;
        private volatile bool _isConnected = false;
        private bool _isDisposed = false;

        /// <summary>
        /// Instantiates a new WebSocket client with specified proxy settings.
        /// </summary>
        /// <param name="proxy">Proxy settings for the client.</param>
        private WebSocketClient(IWebProxy proxy)
        {
            this._connected = new AsyncEvent<WebSocketClient, SocketEventArgs>("WS_CONNECT", this.EventErrorHandler);
            this._disconnected = new AsyncEvent<WebSocketClient, SocketCloseEventArgs>("WS_DISCONNECT", this.EventErrorHandler);
            this._messageReceived = new AsyncEvent<WebSocketClient, SocketMessageEventArgs>("WS_MESSAGE", this.EventErrorHandler);
            this._exceptionThrown = new AsyncEvent<WebSocketClient, SocketErrorEventArgs>("WS_ERROR", null);

            this.Proxy = proxy;
            this._defaultHeaders = new Dictionary<string, string>();
            this.DefaultHeaders = new ReadOnlyDictionary<string, string>(this._defaultHeaders);

            this._receiverTokenSource = null;
            this._receiverToken = CancellationToken.None;
            this._senderLock = new SemaphoreSlim(1);

            this._socketTokenSource = null;
            this._socketToken = CancellationToken.None;
        }

        /// <inheritdoc />
        public async Task ConnectAsync(Uri uri)
        {
            // Disconnect first
            try { await this.DisconnectAsync(); } catch { }

            // Disallow sending messages
            await this._senderLock.WaitAsync();

            try
            {
                // This can be null at this point
                this._receiverTokenSource?.Dispose();
                this._socketTokenSource?.Dispose();

                this._ws?.Dispose();
                this._ws = new ClientWebSocket();
                this._ws.Options.Proxy = this.Proxy;
                this._ws.Options.KeepAliveInterval = TimeSpan.Zero;
                if (this._defaultHeaders != null)
                    foreach (var (k, v) in this._defaultHeaders)
                        this._ws.Options.SetRequestHeader(k, v);

                this._receiverTokenSource = new CancellationTokenSource();
                this._receiverToken = this._receiverTokenSource.Token;

                this._socketTokenSource = new CancellationTokenSource();
                this._socketToken = this._socketTokenSource.Token;

                this._isClientClose = false;
                this._isDisposed = false;
                await this._ws.ConnectAsync(uri, this._socketToken);
                this._receiverTask = Task.Run(this.ReceiverLoopAsync, this._receiverToken);
            }
            finally
            {
                this._senderLock.Release();
            }
        }

        /// <inheritdoc />
        public async Task DisconnectAsync(int code = 1000, string message = "")
        {
            // Ensure that messages cannot be sent
            await this._senderLock.WaitAsync();

            try
            {
                this._isClientClose = true;
                if (this._ws != null && (this._ws.State == WebSocketState.Open || this._ws.State == WebSocketState.CloseReceived))
                    await this._ws.CloseOutputAsync((WebSocketCloseStatus)code, message, CancellationToken.None);

                if (this._receiverTask != null)
                    await this._receiverTask; // Ensure that receiving completed

                if (this._isConnected)
                    this._isConnected = false;

                if (!this._isDisposed)
                {
                    // Cancel all running tasks
                    if (this._socketToken.CanBeCanceled)
                        this._socketTokenSource?.Cancel();
                    this._socketTokenSource?.Dispose();

                    if (this._receiverToken.CanBeCanceled)
                        this._receiverTokenSource?.Cancel();
                    this._receiverTokenSource?.Dispose();

                    this._isDisposed = true;
                }
            }
            catch { }
            finally
            {
                this._senderLock.Release();
            }
        }

        /// <inheritdoc />
        public async Task SendMessageAsync(string message)
        {
            if (this._ws == null)
                return;

            if (this._ws.State != WebSocketState.Open && this._ws.State != WebSocketState.CloseReceived)
                return;

            var bytes = Utilities.UTF8.GetBytes(message);
            await this._senderLock.WaitAsync();
            try
            {
                var len = bytes.Length;
                var segCount = len / OutgoingChunkSize;
                if (len % OutgoingChunkSize != 0)
                    segCount++;

                for (var i = 0; i < segCount; i++)
                {
                    var segStart = OutgoingChunkSize * i;
                    var segLen = Math.Min(OutgoingChunkSize, len - segStart);

                    await this._ws.SendAsync(new ArraySegment<byte>(bytes, segStart, segLen), WebSocketMessageType.Text, i == segCount - 1, CancellationToken.None);
                }
            }
            finally
            {
                this._senderLock.Release();
            }
        }

        /// <inheritdoc />
        public bool AddDefaultHeader(string name, string value)
        {
            this._defaultHeaders[name] = value;
            return true;
        }

        /// <inheritdoc />
        public bool RemoveDefaultHeader(string name)
            => this._defaultHeaders.Remove(name);

        /// <summary>
        /// Disposes of resources used by this WebSocket client instance.
        /// </summary>
        public void Dispose()
        {
            if (this._isDisposed)
                return;

            this._isDisposed = true;

            this.DisconnectAsync().GetAwaiter().GetResult();

            this._receiverTokenSource?.Dispose();
            this._socketTokenSource?.Dispose();
        }

        internal async Task ReceiverLoopAsync()
        {
            await Task.Yield();

            var token = this._receiverToken;
            var buffer = new ArraySegment<byte>(new byte[IncomingChunkSize]);

            try
            {
                using var bs = new MemoryStream();
                while (!token.IsCancellationRequested)
                {
                    // See https://github.com/RogueException/Discord.Net/commit/ac389f5f6823e3a720aedd81b7805adbdd78b66d
                    // for explanation on the cancellation token

                    WebSocketReceiveResult result;
                    byte[] resultBytes;
                    do
                    {
                        result = await this._ws.ReceiveAsync(buffer, CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close)
                            break;

                        bs.Write(buffer.Array, 0, result.Count);
                    }
                    while (!result.EndOfMessage);

                    resultBytes = new byte[bs.Length];
                    bs.Position = 0;
                    bs.Read(resultBytes, 0, resultBytes.Length);
                    bs.Position = 0;
                    bs.SetLength(0);

                    if (!this._isConnected && result.MessageType != WebSocketMessageType.Close)
                    {
                        this._isConnected = true;
                        await this._connected.InvokeAsync(this, new SocketEventArgs());
                    }

                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        await this._messageReceived.InvokeAsync(this, new SocketBinaryMessageEventArgs(resultBytes));
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        await this._messageReceived.InvokeAsync(this, new SocketTextMessageEventArgs(Utilities.UTF8.GetString(resultBytes)));
                    }
                    else // close
                    {
                        if (!this._isClientClose)
                        {
                            var code = result.CloseStatus.Value;
                            code = code == WebSocketCloseStatus.NormalClosure || code == WebSocketCloseStatus.EndpointUnavailable
                                ? (WebSocketCloseStatus)4000
                                : code;

                            await this._ws.CloseOutputAsync(code, result.CloseStatusDescription, CancellationToken.None);
                        }

                        await this._disconnected.InvokeAsync(this, new SocketCloseEventArgs() { CloseCode = (int)result.CloseStatus, CloseMessage = result.CloseStatusDescription });
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                await this._exceptionThrown.InvokeAsync(this, new SocketErrorEventArgs() { Exception = ex });
                await this._disconnected.InvokeAsync(this, new SocketCloseEventArgs() { CloseCode = -1, CloseMessage = "" });
            }

            // Don't await or you deadlock
            // DisconnectAsync waits for this method
            _ = this.DisconnectAsync();
        }

        /// <summary>
        /// Creates a new instance of <see cref="WebSocketClient"/>.
        /// </summary>
        /// <param name="proxy">Proxy to use for this client instance.</param>
        /// <returns>An instance of <see cref="WebSocketClient"/>.</returns>
        public static IWebSocketClient CreateNew(IWebProxy proxy)
            => new WebSocketClient(proxy);

        #region Events
        /// <summary>
        /// Triggered when the client connects successfully.
        /// </summary>
        public event AsyncEventHandler<IWebSocketClient, SocketEventArgs> Connected
        {
            add => this._connected.Register(value);
            remove => this._connected.Unregister(value);
        }
        private readonly AsyncEvent<WebSocketClient, SocketEventArgs> _connected;

        /// <summary>
        /// Triggered when the client is disconnected.
        /// </summary>
        public event AsyncEventHandler<IWebSocketClient, SocketCloseEventArgs> Disconnected
        {
            add => this._disconnected.Register(value);
            remove => this._disconnected.Unregister(value);
        }
        private readonly AsyncEvent<WebSocketClient, SocketCloseEventArgs> _disconnected;

        /// <summary>
        /// Triggered when the client receives a message from the remote party.
        /// </summary>
        public event AsyncEventHandler<IWebSocketClient, SocketMessageEventArgs> MessageReceived
        {
            add => this._messageReceived.Register(value);
            remove => this._messageReceived.Unregister(value);
        }
        private readonly AsyncEvent<WebSocketClient, SocketMessageEventArgs> _messageReceived;

        /// <summary>
        /// Triggered when an error occurs in the client.
        /// </summary>
        public event AsyncEventHandler<IWebSocketClient, SocketErrorEventArgs> ExceptionThrown
        {
            add => this._exceptionThrown.Register(value);
            remove => this._exceptionThrown.Unregister(value);
        }
        private readonly AsyncEvent<WebSocketClient, SocketErrorEventArgs> _exceptionThrown;

        private void EventErrorHandler<TArgs>(AsyncEvent<WebSocketClient, TArgs> asyncEvent, Exception ex, AsyncEventHandler<WebSocketClient, TArgs> handler, WebSocketClient sender, TArgs eventArgs)
            where TArgs : AsyncEventArgs
            => this._exceptionThrown.InvokeAsync(this, new SocketErrorEventArgs() { Exception = ex }).GetAwaiter().GetResult();
        #endregion
    }
}
