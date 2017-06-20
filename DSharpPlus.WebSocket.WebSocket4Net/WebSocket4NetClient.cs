using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using ws4net = WebSocket4Net;

namespace DSharpPlus
{
    public class WebSocket4NetClient : BaseWebSocketClient
    {
        internal static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);
        internal ws4net.WebSocket _socket;

        public WebSocket4NetClient()
        {
            this._connect = new AsyncEvent(this.EventErrorHandler, "WS_CONNECT");
            this._disconnect = new AsyncEvent<SocketDisconnectEventArgs>(this.EventErrorHandler, "WS_DISCONNECT");
            this._message = new AsyncEvent<SocketMessageEventArgs>(this.EventErrorHandler, "WS_MESSAGE");
            this._error = new AsyncEvent<SocketErrorEventArgs>(null, "WS_ERROR");
        }

        public override Task<BaseWebSocketClient> ConnectAsync(string uri)
        {
            _socket = new ws4net.WebSocket(uri);
            _socket.Opened += (sender, e) => _connect.InvokeAsync().GetAwaiter().GetResult();
            _socket.Closed += (sender, e) =>
            {
                if (e is ws4net.ClosedEventArgs ea)
                    _disconnect.InvokeAsync(new SocketDisconnectEventArgs(null) { CloseCode = ea.Code, CloseMessage = ea.Reason }).GetAwaiter().GetResult();
                else
                    _disconnect.InvokeAsync(new SocketDisconnectEventArgs(null) { CloseCode = -1, CloseMessage = "unknown" }).GetAwaiter().GetResult();
            };
            _socket.MessageReceived += (sender, e) => _message.InvokeAsync(new SocketMessageEventArgs()
            {
                Message = e.Message
            }).GetAwaiter().GetResult();
            _socket.DataReceived += (sender, e) =>
            {
                var msg = "";

                using (var ms1 = new MemoryStream(e.Data, 2, e.Data.Length - 2))
                using (var ms2 = new MemoryStream())
                {
                    using (var zlib = new DeflateStream(ms1, CompressionMode.Decompress))
                        zlib.CopyTo(ms2);

                    msg = UTF8.GetString(ms2.ToArray(), 0, (int)ms2.Length);
                }

                _message.InvokeAsync(new SocketMessageEventArgs()
                {
                    Message = msg
                }).GetAwaiter().GetResult();
            };
            _socket.Open();
            return Task.FromResult<BaseWebSocketClient>(this);
        }

        public override Task InternalDisconnectAsync(SocketDisconnectEventArgs e)
        {
            if (_socket.State != ws4net.WebSocketState.Closed)
                _socket.Close();
            return Task.Delay(0);
        }

        public override Task<BaseWebSocketClient> OnConnectAsync()
        {
            return Task.FromResult<BaseWebSocketClient>(this);
        }

        public override Task<BaseWebSocketClient> OnDisconnectAsync(SocketDisconnectEventArgs e)
        {
            return Task.FromResult<BaseWebSocketClient>(this);
        }

        public override void SendMessage(string message)
        {
            if (_socket.State == ws4net.WebSocketState.Open)
                _socket.Send(message);
        }

        public override event AsyncEventHandler OnConnect
        {
            add { this._connect.Register(value); }
            remove { this._connect.Unregister(value); }
        }
        private AsyncEvent _connect;

        public override event AsyncEventHandler<SocketDisconnectEventArgs> OnDisconnect
        {
            add { this._disconnect.Register(value); }
            remove { this._disconnect.Unregister(value); }
        }
        private AsyncEvent<SocketDisconnectEventArgs> _disconnect;

        public override event AsyncEventHandler<SocketMessageEventArgs> OnMessage
        {
            add { this._message.Register(value); }
            remove { this._message.Unregister(value); }
        }
        private AsyncEvent<SocketMessageEventArgs> _message;

        public override event AsyncEventHandler<SocketErrorEventArgs> OnError
        {
            add { this._error.Register(value); }
            remove { this._error.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _error;

        private void EventErrorHandler(string evname, Exception ex)
        {
            if (evname.ToLower() == "ws_error")
                Console.WriteLine($"WSERROR: {ex.GetType()} in {evname}!");
            else
                this._error.InvokeAsync(new SocketErrorEventArgs(null) { Exception = ex }).GetAwaiter().GetResult();
        }
    }
}
