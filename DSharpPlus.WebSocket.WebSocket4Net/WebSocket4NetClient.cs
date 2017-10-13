using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using ws4net = WebSocket4Net;

// ReSharper disable once CheckNamespace
namespace DSharpPlus.Net.WebSocket
{
    public class WebSocket4NetClient : BaseWebSocketClient
    {
        // ReSharper disable once InconsistentNaming
        internal static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);
        internal ws4net.WebSocket Socket;

        public WebSocket4NetClient()
        {
            _connect = new AsyncEvent(EventErrorHandler, "WS_CONNECT");
            _disconnect = new AsyncEvent<SocketCloseEventArgs>(EventErrorHandler, "WS_DISCONNECT");
            _message = new AsyncEvent<SocketMessageEventArgs>(EventErrorHandler, "WS_MESSAGE");
            _error = new AsyncEvent<SocketErrorEventArgs>(null, "WS_ERROR");
        }

        public override Task<BaseWebSocketClient> ConnectAsync(string uri)
        {
            Socket = new ws4net.WebSocket(uri);

            Socket.Opened += (sender, e) => _connect.InvokeAsync().GetAwaiter().GetResult();

            Socket.Closed += (sender, e) =>
            {
                if (e is ws4net.ClosedEventArgs ea)
                {
                    _disconnect.InvokeAsync(new SocketCloseEventArgs(null) { CloseCode = ea.Code, CloseMessage = ea.Reason }).GetAwaiter().GetResult();
                }
                else
                {
                    _disconnect.InvokeAsync(new SocketCloseEventArgs(null) { CloseCode = -1, CloseMessage = "unknown" }).GetAwaiter().GetResult();
                }
            };

            Socket.MessageReceived += (sender, e) => _message.InvokeAsync(new SocketMessageEventArgs()
            {
                Message = e.Message
            }).GetAwaiter().GetResult();

            Socket.DataReceived += (sender, e) =>
            {
                string msg;

                using (var ms1 = new MemoryStream(e.Data, 2, e.Data.Length - 2))
                {
                    using (var ms2 = new MemoryStream())
                    {
                        using (var zlib = new DeflateStream(ms1, CompressionMode.Decompress))
                        {
                            zlib.CopyTo(ms2);
                        }

                        msg = UTF8.GetString(ms2.ToArray(), 0, (int)ms2.Length);
                    }
                }

                _message.InvokeAsync(new SocketMessageEventArgs()
                {
                    Message = msg
                }).GetAwaiter().GetResult();
            };

            Socket.Open();

            return Task.FromResult<BaseWebSocketClient>(this);
        }

        public override Task InternalDisconnectAsync(SocketCloseEventArgs e)
        {
            if (Socket.State != ws4net.WebSocketState.Closed)
            {
                Socket.Close();
            }
            return Task.Delay(0);
        }

        public override Task<BaseWebSocketClient> OnConnectAsync()
        {
            return Task.FromResult<BaseWebSocketClient>(this);
        }

        public override Task<BaseWebSocketClient> OnDisconnectAsync(SocketCloseEventArgs e)
        {
            return Task.FromResult<BaseWebSocketClient>(this);
        }

        public override void SendMessage(string message)
        {
            if (Socket.State == ws4net.WebSocketState.Open)
            {
                Socket.Send(message);
            }
        }

        public override event AsyncEventHandler OnConnect
        {
            add { _connect.Register(value); }
            remove { _connect.Unregister(value); }
        }
        private AsyncEvent _connect;

        public override event AsyncEventHandler<SocketCloseEventArgs> OnDisconnect
        {
            add { _disconnect.Register(value); }
            remove { _disconnect.Unregister(value); }
        }
        private AsyncEvent<SocketCloseEventArgs> _disconnect;

        public override event AsyncEventHandler<SocketMessageEventArgs> OnMessage
        {
            add { _message.Register(value); }
            remove { _message.Unregister(value); }
        }
        private AsyncEvent<SocketMessageEventArgs> _message;

        public override event AsyncEventHandler<SocketErrorEventArgs> OnError
        {
            add { _error.Register(value); }
            remove { _error.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _error;

        private void EventErrorHandler(string evname, Exception ex)
        {
            if (evname.ToLowerInvariant() == "ws_error")
            {
                Console.WriteLine($"WSERROR: {ex.GetType()} in {evname}!");
            }
            else
            {
                _error.InvokeAsync(new SocketErrorEventArgs(null) { Exception = ex }).GetAwaiter().GetResult();
            }
        }
    }
}
