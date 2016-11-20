using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace DSharpPlus
{
    internal class WebSocketClient : IDisposable
    {
        internal event EventHandler SocketOpened;
        internal event EventHandler<CloseEventArgs> SocketClosed;
        internal event EventHandler<MessageEventArgs> SocketMessage;
        internal event EventHandler<ErrorEventArgs> SocketError;

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
            SocketOpened?.Invoke(sender, e);

            DiscordClient._debugLogger.LogMessage(LogLevel.Debug, "WebSocket connection opened", DateTime.Now);
        }

        private void _socket_OnClose(object sender, CloseEventArgs e)
        {
            SocketClosed?.Invoke(sender, e);

            if (e.WasClean)
                DiscordClient._debugLogger.LogMessage(LogLevel.Debug, $"WebSocket connection closed: {e.Reason} [WasClean: {e.WasClean}]", DateTime.Now);
            else
                DiscordClient._debugLogger.LogMessage(LogLevel.Warning, $"WebSocket connection closed: {e.Reason} [WasClean: {e.WasClean}]", DateTime.Now);
        }

        private void _socket_OnMessage(object sender, MessageEventArgs e)
        {
            SocketMessage?.Invoke(sender, e);
        }

        private void _socket_OnError(object sender, ErrorEventArgs e)
        {
            SocketError?.Invoke(sender, e);
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
