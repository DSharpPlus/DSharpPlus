using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using WSS = WebSocket4Net;

namespace DSharpPlus.Net.WebSocket
{
    public class WebSocket4NetClient : BaseWebSocketClient
    {
        private static UTF8Encoding Utf8 { get; } = new UTF8Encoding(false);
        private WSS.WebSocket _socket;

        public override event AsyncEventHandler OnConnect
        {
            add => _connect.Register(value);
            remove => _connect.Unregister(value);
        }
        private readonly AsyncEvent _connect;

        public override event AsyncEventHandler<SocketCloseEventArgs> OnDisconnect
        {
            add => _disconnect.Register(value);
            remove => _disconnect.Unregister(value);
        }
        private readonly AsyncEvent<SocketCloseEventArgs> _disconnect;

        public override event AsyncEventHandler<SocketMessageEventArgs> OnMessage
        {
            add => _message.Register(value);
            remove => _message.Unregister(value);
        }
        private readonly AsyncEvent<SocketMessageEventArgs> _message;

        public override event AsyncEventHandler<SocketErrorEventArgs> OnError
        {
            add => _error.Register(value);
            remove => _error.Unregister(value);
        }
        private readonly AsyncEvent<SocketErrorEventArgs> _error;

        public WebSocket4NetClient()
        {
            _connect = new AsyncEvent(EventErrorHandler, "WS_CONNECT");
            _disconnect = new AsyncEvent<SocketCloseEventArgs>(EventErrorHandler, "WS_DISCONNECT");
            _message = new AsyncEvent<SocketMessageEventArgs>(EventErrorHandler, "WS_MESSAGE");
            _error = new AsyncEvent<SocketErrorEventArgs>(null, "WS_ERROR");
        }

        public override Task<BaseWebSocketClient> ConnectAsync(string uri)
        {
            _socket = new WSS.WebSocket(uri);

            _socket.Opened += (sender, e) => _connect.InvokeAsync().GetAwaiter().GetResult();
                
            _socket.Closed += (sender, e) =>
            {
                if (e is WSS.ClosedEventArgs ea)
                    _disconnect.InvokeAsync(new SocketCloseEventArgs(null) { CloseCode = ea.Code, CloseMessage = ea.Reason }).GetAwaiter().GetResult();
                else
                    _disconnect.InvokeAsync(new SocketCloseEventArgs(null) { CloseCode = -1, CloseMessage = "unknown" }).GetAwaiter().GetResult();
            };

            _socket.MessageReceived += (sender, e) => _message.InvokeAsync(new SocketMessageEventArgs {
                Message = e.Message
            }).GetAwaiter().GetResult();

            _socket.DataReceived += (sender, e) =>
            {
                string msg;

                using (MemoryStream ms1 = new MemoryStream(e.Data, 2, e.Data.Length - 2), ms2 = new MemoryStream())
                {
                    using (var zlib = new DeflateStream(ms1, CompressionMode.Decompress))
                        zlib.CopyTo(ms2);

                    msg = Utf8.GetString(ms2.ToArray(), 0, (int)ms2.Length);
                }

                _message.InvokeAsync(new SocketMessageEventArgs {
                    Message = msg
                }).GetAwaiter().GetResult();
            };

            _socket.Open();

            return Task.FromResult<BaseWebSocketClient>(this);
        }

        public override Task InternalDisconnectAsync(SocketCloseEventArgs e)
        {
            if (_socket.State != WSS.WebSocketState.Closed)
                _socket.Close();
            return Task.Delay(0);
        }

        public override Task<BaseWebSocketClient> OnConnectAsync() => Task.FromResult<BaseWebSocketClient>(this);

        public override Task<BaseWebSocketClient> OnDisconnectAsync(SocketCloseEventArgs e) => Task.FromResult<BaseWebSocketClient>(this);

        public override void SendMessage(string message)
        {
            if (_socket.State == WSS.WebSocketState.Open)
                _socket.Send(message);
        }

        private void EventErrorHandler(string evname, Exception ex)
        {
            if (evname.ToLowerInvariant() == "ws_error")
                Console.WriteLine($"WSERROR: {ex.GetType()} in {evname}!");
            else
                _error.InvokeAsync(new SocketErrorEventArgs(null) { Exception = ex }).GetAwaiter().GetResult();
        }
    }
}
