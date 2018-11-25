using DSharpPlus.EventArgs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            => TokenSource.Token;

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
            _on_connect = new AsyncEvent(EventErrorHandler, "WS_CONNECT");
            _on_disconnect = new AsyncEvent<SocketCloseEventArgs>(EventErrorHandler, "WS_DISCONNECT");
            _on_message = new AsyncEvent<SocketMessageEventArgs>(EventErrorHandler, "WS_MESSAGE");
            _on_error = new AsyncEvent<SocketErrorEventArgs>(null, "WS_ERROR");
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <param name="customHeaders">Custom headers to send with the request.</param>
        /// <returns></returns>
        public override async Task ConnectAsync(Uri uri, IReadOnlyDictionary<string, string> customHeaders = null)
        {
            SocketMessageQueue = new ConcurrentQueue<string>();
            TokenSource = new CancellationTokenSource();

            StreamDecompressor?.Dispose();
            CompressedStream?.Dispose();
            DecompressedStream?.Dispose();

            DecompressedStream = new MemoryStream();
            CompressedStream = new MemoryStream();
            StreamDecompressor = new DeflateStream(CompressedStream, CompressionMode.Decompress);

            Socket = new ClientWebSocket();
            Socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            if (Proxy != null) // because mono doesn't implement this properly
                Socket.Options.Proxy = Proxy;

            if (customHeaders != null)
                foreach (var kvp in customHeaders)
                    Socket.Options.SetRequestHeader(kvp.Key, kvp.Value);

            await Socket.ConnectAsync(uri, Token).ConfigureAwait(false);
            await OnConnectedAsync().ConfigureAwait(false);
            WsListener = Task.Run(ListenAsync, Token);
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
                await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", Token).ConfigureAwait(false);
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
                    var cc = Socket.CloseStatus != null ? (int)Socket.CloseStatus.Value : -1;
                    e = new SocketCloseEventArgs(null) { CloseCode = cc, CloseMessage = Socket.CloseStatusDescription ?? "Unknown reason" };
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

            SocketMessageQueue.Enqueue(message);

            if (SocketQueueManager == null || SocketQueueManager.IsCompleted)
                SocketQueueManager = Task.Run(ProcessSmqAsync, Token);
        }

        /// <summary>
        /// Set the Action to call when the connection has been established.
        /// </summary>
        /// <returns></returns>
        protected override Task OnConnectedAsync() => _on_connect.InvokeAsync();

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <returns></returns>
        protected override Task OnDisconnectedAsync(SocketCloseEventArgs e)
        {
            _ = _on_disconnect.InvokeAsync(e).ConfigureAwait(false);
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

            var token = Token;

            try
            {
                while (!token.IsCancellationRequested && Socket.State == WebSocketState.Open)
                {
                    using (var ms = new MemoryStream())
                    {
                        do
                        {
                            result = await Socket.ReceiveAsync(buffseg, token).ConfigureAwait(false);

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
                            await CompressedStream.WriteAsync(resultbuff, 2, resultbuff.Length - 2).ConfigureAwait(false);
                        else
                            await CompressedStream.WriteAsync(resultbuff, 0, resultbuff.Length).ConfigureAwait(false);
                        await CompressedStream.FlushAsync().ConfigureAwait(false);
                        CompressedStream.Position = 0;

                        // partial credit to FiniteReality
                        // overall idea is his
                        // I tuned the finer details
                        // -Emzi
                        var sfix = BitConverter.ToUInt16(resultbuff, resultbuff.Length - 2);
                        if (sfix != ZLIB_STREAM_SUFFIX)
                        {
                            using (var zlib = new DeflateStream(CompressedStream, CompressionMode.Decompress, true))
                                await zlib.CopyToAsync(DecompressedStream).ConfigureAwait(false);
                        }
                        else
                        {
                            await StreamDecompressor.CopyToAsync(DecompressedStream).ConfigureAwait(false);
                        }

                        resultbuff = DecompressedStream.ToArray();
                        DecompressedStream.Position = 0;
                        DecompressedStream.SetLength(0);
                        CompressedStream.Position = 0;
                        CompressedStream.SetLength(0);
                    }

                    resultstr = UTF8.GetString(resultbuff, 0, resultbuff.Length);
                    await CallOnMessageAsync(resultstr).ConfigureAwait(false);
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

            var token = Token;
            while (!token.IsCancellationRequested && Socket?.State == WebSocketState.Open)
            {
                if (SocketMessageQueue.IsEmpty)
                    break;

                if (!SocketMessageQueue.TryDequeue(out var message))
                    break;

                var buff = UTF8.GetBytes(message);
                var msgc = buff.Length / BUFFER_SIZE;
                if (buff.Length % BUFFER_SIZE != 0)
                    msgc++;

                try
                {
                    for (var i = 0; i < msgc; i++)
                    {
                        var off = BUFFER_SIZE * i;
                        var cnt = Math.Min(BUFFER_SIZE, buff.Length - off);

                        var lm = i == msgc - 1;
                        await Socket.SendAsync(new ArraySegment<byte>(buff, off, cnt), WebSocketMessageType.Text, lm, Token).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    // no debug logger
                    Debug.WriteLine($"Something went very very very very wrong while processing the message queue\r\n{ex}");
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
            add { _on_connect.Register(value); }
            remove { _on_connect.Unregister(value); }
        }
        private AsyncEvent _on_connect;

        /// <summary>
        /// Triggered when the client is disconnected.
        /// </summary>
        public override event AsyncEventHandler<SocketCloseEventArgs> OnDisconnect
        {
            add { _on_disconnect.Register(value); }
            remove { _on_disconnect.Unregister(value); }
        }
        private AsyncEvent<SocketCloseEventArgs> _on_disconnect;

        /// <summary>
        /// Triggered when the client receives a message from the remote party.
        /// </summary>
        public override event AsyncEventHandler<SocketMessageEventArgs> OnMessage
        {
            add { _on_message.Register(value); }
            remove { _on_message.Unregister(value); }
        }
        private AsyncEvent<SocketMessageEventArgs> _on_message;

        /// <summary>
        /// Triggered when an error occurs in the client.
        /// </summary>
        public override event AsyncEventHandler<SocketErrorEventArgs> OnError
        {
            add { _on_error.Register(value); }
            remove { _on_error.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _on_error;

        private void EventErrorHandler(string evname, Exception ex)
        {
            if (evname.ToLowerInvariant() == "ws_error")
                Console.WriteLine($"WSERROR: {ex.GetType()} in {evname}!");
            else
                _on_error.InvokeAsync(new SocketErrorEventArgs(null) { Exception = ex }).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        #endregion
    }
}