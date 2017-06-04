using System;
using System.Threading.Tasks;
using ws4net = WebSocket4Net;

namespace DSharpPlus
{
    public class WebSocket4NetClient : BaseWebSocketClient
    {
        internal ws4net.WebSocket _socket;

        public WebSocket4NetClient()
        {
            this._connect = new AsyncEvent(this.EventErrorHandler, "WS_CONNECT");
            this._disconnect = new AsyncEvent<SocketDisconnectEventArgs>(this.EventErrorHandler, "WS_DISCONNECT");
            this._message = new AsyncEvent<WebSocketMessageEventArgs>(this.EventErrorHandler, "WS_MESSAGE");
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
            _socket.MessageReceived += (sender, e) => _message.InvokeAsync(new WebSocketMessageEventArgs()
            {
                Message = e.Message
            }).GetAwaiter().GetResult();
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

        public override event AsyncEventHandler<WebSocketMessageEventArgs> OnMessage
        {
            add { this._message.Register(value); }
            remove { this._message.Unregister(value); }
        }
        private AsyncEvent<WebSocketMessageEventArgs> _message;

        private void EventErrorHandler(string evname, Exception ex)
        {
            Console.WriteLine($"WSERROR: {ex.GetType()} in {evname}!");
        }
    }
}
