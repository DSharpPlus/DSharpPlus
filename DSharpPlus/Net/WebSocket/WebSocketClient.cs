using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Net.WebSocket
{
    // weebsocket
    /// <summary>
    /// The default, native-based WebSocket client implementation.
    /// </summary>
    public class WebSocketClient : BaseWebSocketClient
    {
        private const int BUFFER_SIZE = 32768;

        private static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);

        private ConcurrentQueue<string> SocketMessageQueue { get; set; }
        private CancellationTokenSource TokenSource { get; set; }
        private CancellationToken Token 
            => this.TokenSource.Token;

        private ClientWebSocket Socket { get; set; }
        private Task WsListener { get; set; }
        private Task SocketQueueManager { get; set; }

        /// <summary>
        /// Instantiates a new WebSocket client with specified proxy settings.
        /// </summary>
        /// <param name="proxy">Proxy settings for the client.</param>
        public WebSocketClient(IWebProxy proxy)
            : base(proxy)
        {
            this._on_connect = new AsyncEvent(this.EventErrorHandler, "WS_CONNECT");
            this._on_disconnect = new AsyncEvent<SocketCloseEventArgs>(this.EventErrorHandler, "WS_DISCONNECT");
            this._on_message = new AsyncEvent<SocketMessageEventArgs>(this.EventErrorHandler, "WS_MESSAGE");
            this._on_error = new AsyncEvent<SocketErrorEventArgs>(null, "WS_ERROR");
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <param name="customHeaders">Custom headers to send with the request.</param>
        /// <returns></returns>
        public override async Task ConnectAsync(Uri uri, IReadOnlyDictionary<string, string> customHeaders = null)
        {
            this.SocketMessageQueue = new ConcurrentQueue<string>();
            this.TokenSource = new CancellationTokenSource();

            this.StreamDecompressor?.Dispose();
            this.CompressedStream?.Dispose();
            this.DecompressedStream?.Dispose();

            this.DecompressedStream = new MemoryStream();
            this.CompressedStream = new MemoryStream();
            this.StreamDecompressor = new DeflateStream(this.CompressedStream, CompressionMode.Decompress);
            
            this.Socket = new ClientWebSocket();
            this.Socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            if (this.Proxy != null) // because mono doesn't implement this properly
                this.Socket.Options.Proxy = this.Proxy;

            if (customHeaders != null)
                foreach (var kvp in customHeaders)
                    this.Socket.Options.SetRequestHeader(kvp.Key, kvp.Value);
        
            await Socket.ConnectAsync(uri, this.Token).ConfigureAwait(false);
            await OnConnectedAsync().ConfigureAwait(false);
            this.WsListener = Task.Run(this.ListenAsync, this.Token);
        }

        private bool close_requested = false;
        /// <summary>
        /// Disconnects the WebSocket connection.
        /// </summary>
        /// <param name="e">Disconect event arguments.</param>
        /// <returns></returns>
        public override async Task DisconnectAsync(SocketCloseEventArgs e)
        {
            //if (this.Socket.State != WebSocketState.Open || this.Token.IsCancellationRequested)
            if (close_requested)
                return;

            close_requested = true;
            try
            {
                await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", this.Token).ConfigureAwait(false);
                e = e ?? new SocketCloseEventArgs(null) { CloseCode = (int)WebSocketCloseStatus.NormalClosure, CloseMessage = "" };
                Socket.Abort();
                Socket.Dispose();
            }
            catch (Exception)
            { }
            finally
            {
                if (e == null)
                {
                    var cc = this.Socket.CloseStatus != null ? (int)this.Socket.CloseStatus.Value : -1;
                    e = new SocketCloseEventArgs(null) { CloseCode = cc, CloseMessage = this.Socket.CloseStatusDescription ?? "Unknown reason" };
                }
                await OnDisconnectedAsync(e).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Send a message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public override void SendMessage(string message)
        {
            if (Socket.State != WebSocketState.Open)
                return;

            this.SocketMessageQueue.Enqueue(message);

            if (this.SocketQueueManager == null || this.SocketQueueManager.IsCompleted)
                this.SocketQueueManager = Task.Run(this.ProcessSmqAsync, this.Token);
        }

        /// <summary>
        /// Set the Action to call when the connection has been established.
        /// </summary>
        /// <returns></returns>
        protected override Task OnConnectedAsync()
        {
            return _on_connect.InvokeAsync();
        }

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <returns></returns>
        protected override Task OnDisconnectedAsync(SocketCloseEventArgs e)
        {
            _ = this._on_disconnect.InvokeAsync(e).ConfigureAwait(false);
            return Task.Delay(0);
        }

        internal async Task ListenAsync()
        {
            await Task.Yield();

            var buff = new byte[BUFFER_SIZE];
            var buffseg = new ArraySegment<byte>(buff);

            byte[] resultbuff = null;
            WebSocketReceiveResult result = null;
            SocketCloseEventArgs close = null;

            var token = this.Token;
            
            try
            {
                while (!token.IsCancellationRequested && this.Socket.State == WebSocketState.Open)
                {
                    using (var ms = new MemoryStream())
                    {
                        do
                        {
                            result = await this.Socket.ReceiveAsync(buffseg, token).ConfigureAwait(false);

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                var cc = result.CloseStatus != null ? (int)result.CloseStatus.Value : -1;
                                close = new SocketCloseEventArgs(null) { CloseCode = cc, CloseMessage = result.CloseStatusDescription };
                            }
                            else
                                ms.Write(buff, 0, result.Count);
                        }
                        while (!result.EndOfMessage);

                        resultbuff = ms.ToArray();
                    }

                    if (close != null)
                        break;

                    var resultstr = "";
                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        if (resultbuff[0] == 0x78)
                            await this.CompressedStream.WriteAsync(resultbuff, 2, resultbuff.Length - 2).ConfigureAwait(false);
                        else
                            await this.CompressedStream.WriteAsync(resultbuff, 0, resultbuff.Length).ConfigureAwait(false);
                        await this.CompressedStream.FlushAsync().ConfigureAwait(false);
                        this.CompressedStream.Position = 0;

                        // partial credit to FiniteReality
                        // overall idea is his
                        // I tuned the finer details
                        // -Emzi
                        var sfix = BitConverter.ToUInt16(resultbuff, resultbuff.Length - 2);
                        if (sfix != ZLIB_STREAM_SUFFIX)
                        {
                            using (var zlib = new DeflateStream(this.CompressedStream, CompressionMode.Decompress, true))
                                await zlib.CopyToAsync(this.DecompressedStream).ConfigureAwait(false);
                        }
                        else
                        {
                            await this.StreamDecompressor.CopyToAsync(this.DecompressedStream).ConfigureAwait(false);
                        }

                        resultbuff = this.DecompressedStream.ToArray();
                        this.DecompressedStream.Position = 0;
                        this.DecompressedStream.SetLength(0);
                        this.CompressedStream.Position = 0;
                        this.CompressedStream.SetLength(0);
                    }
                    
                    resultstr = UTF8.GetString(resultbuff, 0, resultbuff.Length);
                    await this.CallOnMessageAsync(resultstr).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                close = new SocketCloseEventArgs(null) { CloseCode = -1, CloseMessage = e.Message };
            }

            await DisconnectAsync(close).ConfigureAwait(false);
        }

        internal async Task ProcessSmqAsync()
        {
            await Task.Yield();

            var token = this.Token;
            while (!token.IsCancellationRequested && this.Socket.State == WebSocketState.Open)
            {
                if (this.SocketMessageQueue.IsEmpty)
                    break;

                if (!this.SocketMessageQueue.TryDequeue(out var message))
                    break;

                var buff = UTF8.GetBytes(message);
                var msgc = buff.Length / BUFFER_SIZE;
                if (buff.Length % BUFFER_SIZE != 0)
                    msgc++;

                for (var i = 0; i < msgc; i++)
                {
                    var off = BUFFER_SIZE * i;
                    var cnt = Math.Min(BUFFER_SIZE, buff.Length - off);

                    var lm = i == msgc - 1;
                    await Socket.SendAsync(new ArraySegment<byte>(buff, off, cnt), WebSocketMessageType.Text, lm, this.Token).ConfigureAwait(false);
                }
            }
        }

        internal Task CallOnMessageAsync(string result)
            => _on_message.InvokeAsync(new SocketMessageEventArgs() { Message = result });

        /// <summary>
        /// Creates a new instance of <see cref="WebSocketClient"/>.
        /// </summary>
        /// <param name="proxy">Proxy to use for this client instance.</param>
        /// <returns>An instance of <see cref="WebSocketClient"/>.</returns>
        public static BaseWebSocketClient CreateNew(IWebProxy proxy)
            => new WebSocketClient(proxy);

        #region Events
        /// <summary>
        /// Triggered when the client connects successfully.
        /// </summary>
        public override event AsyncEventHandler OnConnect
        {
            add { this._on_connect.Register(value); }
            remove { this._on_connect.Unregister(value); }
        }
        private AsyncEvent _on_connect;

        /// <summary>
        /// Triggered when the client is disconnected.
        /// </summary>
        public override event AsyncEventHandler<SocketCloseEventArgs> OnDisconnect
        {
            add { this._on_disconnect.Register(value); }
            remove { this._on_disconnect.Unregister(value); }
        }
        private AsyncEvent<SocketCloseEventArgs> _on_disconnect;

        /// <summary>
        /// Triggered when the client receives a message from the remote party.
        /// </summary>
        public override event AsyncEventHandler<SocketMessageEventArgs> OnMessage
        {
            add { this._on_message.Register(value); }
            remove { this._on_message.Unregister(value); }
        }
        private AsyncEvent<SocketMessageEventArgs> _on_message;

        /// <summary>
        /// Triggered when an error occurs in the client.
        /// </summary>
        public override event AsyncEventHandler<SocketErrorEventArgs> OnError
        {
            add { this._on_error.Register(value); }
            remove { this._on_error.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _on_error;

        private void EventErrorHandler(string evname, Exception ex)
        {
            if (evname.ToLowerInvariant() == "ws_error")
                Console.WriteLine($"WSERROR: {ex.GetType()} in {evname}!");
            else
                this._on_error.InvokeAsync(new SocketErrorEventArgs(null) { Exception = ex }).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        #endregion
    }
}