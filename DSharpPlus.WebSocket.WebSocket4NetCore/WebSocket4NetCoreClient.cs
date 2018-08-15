using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using s = System;
using ws4net = WebSocket4Net;

namespace DSharpPlus.Net.WebSocket
{
    /// <summary>
    /// A WebSocket4Net-based WebSocket client implementation.
    /// </summary>
    public class WebSocket4NetCoreClient : BaseWebSocketClient
    {
        private static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);
        private ws4net.WebSocket _socket;

        /// <summary>
        /// Creates a new WebSocket client.
        /// </summary>
        public WebSocket4NetCoreClient(IWebProxy proxy)
            : base(proxy)
        {
            this._connect = new AsyncEvent(EventErrorHandler, "WS_CONNECT");
            this._disconnect = new AsyncEvent<SocketCloseEventArgs>(EventErrorHandler, "WS_DISCONNECT");
            this._message = new AsyncEvent<SocketMessageEventArgs>(EventErrorHandler, "WS_MESSAGE");
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

            this._socket = new ws4net.WebSocket(uri.ToString(), customHeaderItems: customHeaders?.ToList());
            if (this.Proxy != null) // fuck this, I ain't working with that shit
                throw new NotImplementedException("Proxies are not supported on non-Microsoft WebSocket client implementations.");

            this._socket.Opened += HandlerOpen;
            this._socket.Closed += HandlerClose;
            this._socket.MessageReceived += HandlerMessage;
            this._socket.DataReceived += HandlerData;

            this._socket.Open();
            return Task.FromResult<BaseWebSocketClient>(this);

            void HandlerOpen(object sender, s.EventArgs e)
                => _connect.InvokeAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            void HandlerClose(object sender, s.EventArgs e)
            {
                if (e is ws4net.ClosedEventArgs ea)
                    this._disconnect.InvokeAsync(new SocketCloseEventArgs(null) { CloseCode = ea.Code, CloseMessage = ea.Reason }).ConfigureAwait(false).GetAwaiter().GetResult();
                else
                    this._disconnect.InvokeAsync(new SocketCloseEventArgs(null) { CloseCode = -1, CloseMessage = "unknown" }).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            void HandlerMessage(object sender, ws4net.MessageReceivedEventArgs e)
                => _message.InvokeAsync(new SocketMessageEventArgs { Message = e.Message }).ConfigureAwait(false).GetAwaiter().GetResult();

            void HandlerData(object sender, ws4net.DataReceivedEventArgs e)
            {
                string msg;

                if (e.Data[0] == 0x78)
                    this.CompressedStream.Write(e.Data, 2, e.Data.Length - 2);
                else
                    this.CompressedStream.Write(e.Data, 0, e.Data.Length);
                this.CompressedStream.Flush();
                this.CompressedStream.Position = 0;

                // partial credit to FiniteReality
                // overall idea is his
                // I tuned the finer details
                // -Emzi
                var sfix = BitConverter.ToUInt16(e.Data, e.Data.Length - 2);
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

                this._message.InvokeAsync(new SocketMessageEventArgs { Message = msg }).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Disconnects the WebSocket connection.
        /// </summary>
        /// <param name="e">Disconect event arguments.</param>
        /// <returns></returns>
        public override Task DisconnectAsync(SocketCloseEventArgs e)
        {
            if (this._socket.State != ws4net.WebSocketState.Closed)
                this._socket.Close();
            return Task.Delay(0);
        }

        /// <summary>
        /// Send a message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public override void SendMessage(string message)
        {
            if (this._socket.State == ws4net.WebSocketState.Open)
                this._socket.Send(message);
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
        /// Creates a new instance of <see cref="WebSocket4NetCoreClient"/>.
        /// </summary>
        /// <param name="proxy">Proxy to use for this client instance.</param>
        /// <returns>An instance of <see cref="WebSocket4NetCoreClient"/>.</returns>
        public static BaseWebSocketClient CreateNew(IWebProxy proxy)
            => new WebSocket4NetCoreClient(proxy);

        #region Events
        /// <summary>
        /// Triggered when the client connects successfully.
        /// </summary>
        public override event AsyncEventHandler OnConnect
        {
            add { _connect.Register(value); }
            remove { _connect.Unregister(value); }
        }
        private readonly AsyncEvent _connect;

        /// <summary>
        /// Triggered when the client is disconnected.
        /// </summary>
        public override event AsyncEventHandler<SocketCloseEventArgs> OnDisconnect
        {
            add { _disconnect.Register(value); }
            remove { _disconnect.Unregister(value); }
        }
        private readonly AsyncEvent<SocketCloseEventArgs> _disconnect;

        /// <summary>
        /// Triggered when the client receives a message from the remote party.
        /// </summary>
        public override event AsyncEventHandler<SocketMessageEventArgs> OnMessage
        {
            add { _message.Register(value); }
            remove { _message.Unregister(value); }
        }
        private readonly AsyncEvent<SocketMessageEventArgs> _message;

        /// <summary>
        /// Triggered when an error occurs in the client.
        /// </summary>
        public override event AsyncEventHandler<SocketErrorEventArgs> OnError
        {
            add { _error.Register(value); }
            remove { _error.Unregister(value); }
        }
        private readonly AsyncEvent<SocketErrorEventArgs> _error;

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
