using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using wss = WebSocketSharp;

namespace DSharpPlus
{
    /// <summary>
    /// A WS#-based WebSocket client.
    /// </summary>
    public class WebSocketSharpClient : BaseWebSocketClient
    {
        internal static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);
        internal wss.WebSocket _socket;

        public WebSocketSharpClient()
        {
            this._connect = new AsyncEvent(this.EventErrorHandler, "WS_CONNECT");
            this._disconnect = new AsyncEvent<SocketDisconnectEventArgs>(this.EventErrorHandler, "WS_DISCONNECT");
            this._message = new AsyncEvent<SocketMessageEventArgs>(this.EventErrorHandler, "WS_MESSAGE");
            this._error = new AsyncEvent<SocketErrorEventArgs>(null, "WS_ERROR");
        }

        public override Task<BaseWebSocketClient> ConnectAsync(string uri)
        {
            _socket = new wss.WebSocket(uri);
            _socket.OnOpen += (sender, e) => _connect.InvokeAsync().GetAwaiter().GetResult();
            _socket.OnClose += (sender, e) => _disconnect.InvokeAsync(new SocketDisconnectEventArgs(null) { CloseCode = e.Code, CloseMessage = e.Reason }).GetAwaiter().GetResult();
            _socket.OnMessage += (sender, e) =>
            {
                var msg = "";

                if (e.IsBinary)
                {
                    using (var ms1 = new MemoryStream(e.RawData, 2, e.RawData.Length - 2))
                    using (var ms2 = new MemoryStream())
                    {
                        using (var zlib = new DeflateStream(ms1, CompressionMode.Decompress))
                            zlib.CopyTo(ms2);

                        msg = UTF8.GetString(ms2.ToArray(), 0, (int)ms2.Length);
                    }
                }
                else
                    msg = e.Data;

                _message.InvokeAsync(new SocketMessageEventArgs()
                {
                    Message = msg
                }).GetAwaiter().GetResult();
            };
            _socket.Connect();
            return Task.FromResult<BaseWebSocketClient>(this);
        }

        public override Task InternalDisconnectAsync(SocketDisconnectEventArgs e)
        {
            if (_socket.IsAlive)
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
            if (_socket.IsAlive)
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
            remove { this._disconnect.Unregister(value);  }
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