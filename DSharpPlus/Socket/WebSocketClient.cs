using System;
using WebSocketSharp;

namespace DSharpPlus
{
    internal class WebSocketClient : IDisposable
    {
        internal event AsyncEventHandler SocketOpened
        {
            add => this._socket_opened.Register(value);
            remove => this._socket_opened.Unregister(value);
        }
        internal event AsyncEventHandler<CloseEventArgs> SocketClosed
        {
            add => this._socket_closed.Register(value);
            remove => this._socket_closed.Unregister(value);
        }
        internal event AsyncEventHandler<MessageEventArgs> SocketMessage
        {
            add => this._socket_message.Register(value);
            remove => this._socket_message.Unregister(value);
        }
        internal event AsyncEventHandler<ErrorEventArgs> SocketError
        {
            add => this._socket_error.Register(value);
            remove => this._socket_error.Unregister(value);
        }

        private AsyncEvent _socket_opened = new AsyncEvent();
        private AsyncEvent<CloseEventArgs> _socket_closed = new AsyncEvent<CloseEventArgs>();
        private AsyncEvent<MessageEventArgs> _socket_message = new AsyncEvent<MessageEventArgs>();
        private AsyncEvent<ErrorEventArgs> _socket_error = new AsyncEvent<ErrorEventArgs>();

        internal WebSocket _socket;

        internal WebSocketClient(string gatewayurl)
        {
            _socket = new WebSocket(gatewayurl);
            _socket.OnOpen += _socket_OnOpen;
            _socket.OnClose += _socket_OnClose;
            _socket.OnMessage += _socket_OnMessage;
            _socket.OnError += _socket_OnError;
        }

        internal void Connect()
        {
            _socket.Connect();
        }

        internal void Disconnect()
        {
            if (_socket.IsAlive)
                _socket.Close();
        }

        private void _socket_OnOpen(object sender, EventArgs e)
        {
            this._socket_opened.InvokeAsync().GetAwaiter().GetResult();
        }

        private void _socket_OnClose(object sender, CloseEventArgs e)
        {
            this._socket_closed.InvokeAsync(e).GetAwaiter().GetResult();
        }

        private void _socket_OnMessage(object sender, MessageEventArgs e)
        {
            this._socket_message.InvokeAsync(e).GetAwaiter().GetResult();
        }

        private void _socket_OnError(object sender, ErrorEventArgs e)
        {
            this._socket_error.InvokeAsync(e).GetAwaiter().GetResult();
        }

        ~WebSocketClient()
        {
            Dispose();
        }

        private bool disposed;
        public void Dispose()
        {
            if (disposed)
                return;

            GC.SuppressFinalize(this);

            Disconnect();
            _socket.Close();
            _socket = null;

            disposed = true;
        }
    }
}
