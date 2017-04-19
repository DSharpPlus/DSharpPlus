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

        public WebSocketSharpClient() { }

        public override Task<BaseWebSocketClient> ConnectAsync(string uri)
        {
            _socket = new wss.WebSocket(uri);
            _socket.OnOpen += (sender, e) => _connect.InvokeAsync().GetAwaiter().GetResult();
            _socket.OnClose += (sender, e) => _disconnect.InvokeAsync().GetAwaiter().GetResult();
            _socket.OnMessage += (sender, e) => _message.InvokeAsync(new WebSocketMessageEventArgs()
            {
                Message = e.Data
            }).GetAwaiter().GetResult();
            _socket.Connect();
            return Task.FromResult<BaseWebSocketClient>(this);
        }

        public override Task InternalDisconnectAsync()
        {
            if (_socket.IsAlive)
                _socket.Close();
            return Task.Delay(0);
        }

        public override Task<BaseWebSocketClient> OnConnectAsync()
        {
            return Task.FromResult<BaseWebSocketClient>(this);
        }

        public override Task<BaseWebSocketClient> OnDisconnectAsync()
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
        private AsyncEvent _connect = new AsyncEvent();

        public override event AsyncEventHandler OnDisconnect
        {
            add { this._disconnect.Register(value); }
            remove { this._disconnect.Unregister(value);  }
        }
        private AsyncEvent _disconnect = new AsyncEvent();

        public override event AsyncEventHandler<WebSocketMessageEventArgs> OnMessage
        {
            add { this._message.Register(value); }
            remove { this._message.Unregister(value); }
        }
        private AsyncEvent<WebSocketMessageEventArgs> _message = new AsyncEvent<WebSocketMessageEventArgs>();
    }
}