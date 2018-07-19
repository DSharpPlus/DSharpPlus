using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using s = System;
using wss = WebSocketSharp;

namespace DSharpPlus.Net.WebSocket
{
    /// <summary>
    /// A WebSocketSharp-based WebSocket client implementation.
    /// </summary>
    public class WebSocketSharpClient : BaseWebSocketClient
    {
        internal static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);
        internal wss.WebSocket _socket;

        /// <summary>
        /// Creates a new WebSocket client.
        /// </summary>
        public WebSocketSharpClient(IWebProxy proxy)
            : base(proxy)
        {
            this._connect = new AsyncEvent(this.EventErrorHandler, "WS_CONNECT");
            this._disconnect = new AsyncEvent<SocketCloseEventArgs>(this.EventErrorHandler, "WS_DISCONNECT");
            this._message = new AsyncEvent<SocketMessageEventArgs>(this.EventErrorHandler, "WS_MESSAGE");
            this._error = new AsyncEvent<SocketErrorEventArgs>(null, "WS_ERROR");
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <param name="customHeaders">Custom headers to send with the request.</param>
        /// <returns></returns>
        public override Task ConnectAsync(Uri uri, IReadOnlyDictionary<string, string> customHeaders = null)
        {
            this.StreamDecompressor?.Dispose();
            this.CompressedStream?.Dispose();
            this.DecompressedStream?.Dispose();

            this.DecompressedStream = new MemoryStream();
            this.CompressedStream = new MemoryStream();
            this.StreamDecompressor = new DeflateStream(this.CompressedStream, CompressionMode.Decompress);

            _socket = new wss.WebSocket(uri.ToString());
            if (this.Proxy != null) // fuck this, I ain't working with that shit
                throw new NotImplementedException("Proxies are not supported on non-Microsoft WebSocket client implementations.");

            if (customHeaders != null && customHeaders.Count > 0) // not implemented in the library
                throw new NotSupportedException("WS# client does not support specifying custom headers.");

            _socket.OnOpen += HandlerOpen;
            _socket.OnClose += HandlerClose;
            _socket.OnMessage += HandlerMessage;

            _socket.Connect();
            return Task.FromResult<BaseWebSocketClient>(this);

            void HandlerOpen(object sender, s.EventArgs e)
                => _connect.InvokeAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            void HandlerClose(object sender, wss.CloseEventArgs e)
                => _disconnect.InvokeAsync(new SocketCloseEventArgs(null) { CloseCode = e.Code, CloseMessage = e.Reason }).ConfigureAwait(false).GetAwaiter().GetResult();

            void HandlerMessage(object sender, wss.MessageEventArgs e)
            {
                var msg = "";

                if (e.IsBinary)
                {
                    if (e.RawData[0] == 0x78)
                        this.CompressedStream.Write(e.RawData, 2, e.RawData.Length - 2);
                    else
                        this.CompressedStream.Write(e.RawData, 0, e.RawData.Length);
                    this.CompressedStream.Flush();
                    this.CompressedStream.Position = 0;

                    // partial credit to FiniteReality
                    // overall idea is his
                    // I tuned the finer details
                    // -Emzi
                    var sfix = BitConverter.ToUInt16(e.RawData, e.RawData.Length - 2);
                    if (sfix != ZLIB_STREAM_SUFFIX)
                    {
                        using (var zlib = new DeflateStream(this.CompressedStream, CompressionMode.Decompress, true))
                            zlib.CopyTo(this.DecompressedStream);
                    }
                    else
                    {
                        this.StreamDecompressor.CopyTo(this.DecompressedStream);
                    }

                    msg = UTF8.GetString(this.DecompressedStream.ToArray(), 0, (int)this.DecompressedStream.Length);

                    this.DecompressedStream.Position = 0;
                    this.DecompressedStream.SetLength(0);
                    this.CompressedStream.Position = 0;
                    this.CompressedStream.SetLength(0);
                }
                else
                    msg = e.Data;

                _message.InvokeAsync(new SocketMessageEventArgs() { Message = msg }).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Disconnects the WebSocket connection.
        /// </summary>
        /// <param name="e">Disconect event arguments.</param>
        /// <returns></returns>
        public override Task DisconnectAsync(SocketCloseEventArgs e)
        {
            if (_socket.IsAlive)
                _socket.Close();
            return Task.Delay(0);
        }

        /// <summary>
        /// Send a message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public override void SendMessage(string message)
        {
            if (_socket.IsAlive)
                _socket.Send(message);
        }

        /// <summary>
        /// Set the Action to call when the connection has been established.
        /// </summary>
        /// <returns></returns>
        protected override Task OnConnectedAsync()
            => Task.Delay(0);

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <returns></returns>
        protected override Task OnDisconnectedAsync(SocketCloseEventArgs e)
            => Task.Delay(0);

        /// <summary>
        /// Creates a new instance of <see cref="WebSocketSharpClient"/>.
        /// </summary>
        /// <param name="proxy">Proxy to use for this client instance.</param>
        /// <returns>An instance of <see cref="WebSocketSharpClient"/>.</returns>
        public static BaseWebSocketClient CreateNew(IWebProxy proxy)
            => new WebSocketSharpClient(proxy);

        #region Events
        /// <summary>
        /// Triggered when the client connects successfully.
        /// </summary>
        public override event AsyncEventHandler OnConnect
        {
            add { this._connect.Register(value); }
            remove { this._connect.Unregister(value); }
        }
        private AsyncEvent _connect;

        /// <summary>
        /// Triggered when the client is disconnected.
        /// </summary>
        public override event AsyncEventHandler<SocketCloseEventArgs> OnDisconnect
        {
            add { this._disconnect.Register(value); }
            remove { this._disconnect.Unregister(value);  }
        }
        private AsyncEvent<SocketCloseEventArgs> _disconnect;

        /// <summary>
        /// Triggered when the client receives a message from the remote party.
        /// </summary>
        public override event AsyncEventHandler<SocketMessageEventArgs> OnMessage
        {
            add { this._message.Register(value); }
            remove { this._message.Unregister(value); }
        }
        private AsyncEvent<SocketMessageEventArgs> _message;

        /// <summary>
        /// Triggered when an error occurs in the client.
        /// </summary>
        public override event AsyncEventHandler<SocketErrorEventArgs> OnError
        {
            add { this._error.Register(value); }
            remove { this._error.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _error;

        private void EventErrorHandler(string evname, Exception ex)
        {
            if (evname.ToLowerInvariant() == "ws_error")
                Console.WriteLine($"WSERROR: {ex.GetType()} in {evname}!");
            else
                this._error.InvokeAsync(new SocketErrorEventArgs(null) { Exception = ex }).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        #endregion
    }
}