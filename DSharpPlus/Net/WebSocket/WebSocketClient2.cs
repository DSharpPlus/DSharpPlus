using System;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Net.WebSocket
{
    public class WebSocketClient : BaseWebSocketClient
    {
#pragma warning disable 649
        public override event AsyncEventHandler OnConnect
        {
            add { this._on_connect.Register(value); }
            remove { this._on_connect.Unregister(value); }
        }
        private AsyncEvent _on_connect;

        public override event AsyncEventHandler<SocketCloseEventArgs> OnDisconnect
        {
            add { this._on_disconnect.Register(value); }
            remove { this._on_disconnect.Unregister(value); }
        }
        private AsyncEvent<SocketCloseEventArgs> _on_disconnect;

        public override event AsyncEventHandler<SocketMessageEventArgs> OnMessage
        {
            add { this._on_message.Register(value); }
            remove { this._on_message.Unregister(value); }
        }
        private AsyncEvent<SocketMessageEventArgs> _on_message;

        public override event AsyncEventHandler<SocketErrorEventArgs> OnError
        {
            add { this._on_error.Register(value); }
            remove { this._on_error.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _on_error;
#pragma warning restore 649

        public WebSocketClient()
        {
            throw new PlatformNotSupportedException("Microsoft WebSocket provider is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <returns></returns>
        public static new WebSocketClient Create()
        {
            throw new PlatformNotSupportedException("Microsoft WebSocket provider is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <returns></returns>
        public override Task<BaseWebSocketClient> ConnectAsync(string uri)
        {
            throw new PlatformNotSupportedException("Microsoft WebSocket provider is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Set the Action to call when the connection has been established.
        /// </summary>
        /// <returns></returns>
        public override Task<BaseWebSocketClient> OnConnectAsync()
        {
            throw new PlatformNotSupportedException("Microsoft WebSocket provider is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <returns></returns>
        public override Task<BaseWebSocketClient> OnDisconnectAsync(SocketCloseEventArgs e)
        {
            throw new PlatformNotSupportedException("Microsoft WebSocket provider is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Send a message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public override void SendMessage(string message)
        {
            throw new PlatformNotSupportedException("Microsoft WebSocket provider is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        public override Task InternalDisconnectAsync(SocketCloseEventArgs e)
        {
            throw new PlatformNotSupportedException("Microsoft WebSocket provider is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }
    }
}