using System;
using System.Threading.Tasks;
using wss = WebSocketSharp;

namespace DSharpPlus
{
    /// <summary>
    /// A WS#-based WebSocket client.
    /// </summary>
    public class WebSocketSharpClient : BaseWebSocketClient
    {
        internal wss.WebSocket _socket;

        public WebSocketSharpClient()
        {
            this._connect = new AsyncEvent(this.EventErrorHandler, "WS_CONNECT");
            this._disconnect = new AsyncEvent<SocketDisconnectEventArgs>(this.EventErrorHandler, "WS_DISCONNECT");
            this._message = new AsyncEvent<WebSocketMessageEventArgs>(this.EventErrorHandler, "WS_MESSAGE");
        }

        public override Task<BaseWebSocketClient> ConnectAsync(string uri)
        {
            _socket = new wss.WebSocket(uri);
            _socket.OnOpen += (sender, e) => _connect.InvokeAsync().GetAwaiter().GetResult();
            _socket.OnClose += (sender, e) => _disconnect.InvokeAsync(new SocketDisconnectEventArgs(null) { CloseCode = e.Code, CloseMessage = e.Reason }).GetAwaiter().GetResult();
            _socket.OnMessage += (sender, e) => _message.InvokeAsync(new WebSocketMessageEventArgs()
            {
                Message = e.Data
            }).GetAwaiter().GetResult();
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