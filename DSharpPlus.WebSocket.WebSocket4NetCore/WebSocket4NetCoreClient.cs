using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using ws4net = WebSocket4Net;
using s = System;

namespace DSharpPlus.Net.WebSocket
{
    public class WebSocket4NetCoreClient : BaseWebSocketClient
    {
        private static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);
        private ws4net.WebSocket _socket;

        public override event AsyncEventHandler OnConnect
        {
            add { _connect.Register(value); }
            remove { _connect.Unregister(value); }
        }
        private readonly AsyncEvent _connect;

        public override event AsyncEventHandler<SocketCloseEventArgs> OnDisconnect
        {
            add { _disconnect.Register(value); }
            remove { _disconnect.Unregister(value); }
        }
        private readonly AsyncEvent<SocketCloseEventArgs> _disconnect;

        public override event AsyncEventHandler<SocketMessageEventArgs> OnMessage
        {
            add { _message.Register(value); }
            remove { _message.Unregister(value); }
        }
        private readonly AsyncEvent<SocketMessageEventArgs> _message;

        public override event AsyncEventHandler<SocketErrorEventArgs> OnError
        {
            add { _error.Register(value); }
            remove { _error.Unregister(value); }
        }
        private readonly AsyncEvent<SocketErrorEventArgs> _error;

        public WebSocket4NetCoreClient()
        {
            this._connect = new AsyncEvent(EventErrorHandler, "WS_CONNECT");
            this._disconnect = new AsyncEvent<SocketCloseEventArgs>(EventErrorHandler, "WS_DISCONNECT");
            this._message = new AsyncEvent<SocketMessageEventArgs>(EventErrorHandler, "WS_MESSAGE");
            this._error = new AsyncEvent<SocketErrorEventArgs>(null, "WS_ERROR");
        }

        public override Task<BaseWebSocketClient> ConnectAsync(Uri uri)
        {
            this.StreamDecompressor?.Dispose();
            this.CompressedStream?.Dispose();
            this.DecompressedStream?.Dispose();

            this.DecompressedStream = new MemoryStream();
            this.CompressedStream = new MemoryStream();
            this.StreamDecompressor = new DeflateStream(this.CompressedStream, CompressionMode.Decompress);

            this._socket = new ws4net.WebSocket(uri.ToString());

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

        public override Task DisconnectAsync(SocketCloseEventArgs e)
        {
            if (this._socket.State != ws4net.WebSocketState.Closed)
                this._socket.Close();
            return Task.Delay(0);
        }

        public override Task<BaseWebSocketClient> OnConnectAsync()
            => Task.FromResult<BaseWebSocketClient>(this);

        public override Task<BaseWebSocketClient> OnDisconnectAsync(SocketCloseEventArgs e)
            => Task.FromResult<BaseWebSocketClient>(this);

        public override void SendMessage(string message)
        {
            if (this._socket.State == ws4net.WebSocketState.Open)
                this._socket.Send(message);
        }

        private void EventErrorHandler(string evname, Exception ex)
        {
            if (evname.ToLowerInvariant() == "ws_error")
                Console.WriteLine($"WSERROR: {ex.GetType()} in {evname}!");
            else
                this._error.InvokeAsync(new SocketErrorEventArgs(null) { Exception = ex }).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
