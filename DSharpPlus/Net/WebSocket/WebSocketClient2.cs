using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Net.WebSocket
{
    /// <summary>
    /// The default, native-based WebSocket client implementation.
    /// </summary>
    public class WebSocketClient : BaseWebSocketClient
    {
        /// <summary>
        /// Instantiates a new WebSocket client with specified proxy settings.
        /// </summary>
        /// <param name="proxy">Proxy settings for the client.</param>
        public WebSocketClient(IWebProxy proxy)
            : base(proxy)
        {
            throw new PlatformNotSupportedException("Microsoft WebSocket client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <param name="customHeaders">Custom headers to send with the request.</param>
        /// <returns></returns>
        public override Task ConnectAsync(Uri uri, IReadOnlyDictionary<string, string> customHeaders = null)
        {
            throw new PlatformNotSupportedException("Microsoft WebSocket client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Disconnects the WebSocket connection.
        /// </summary>
        /// <param name="e">Disconect event arguments.</param>
        /// <returns></returns>
        public override Task DisconnectAsync(SocketCloseEventArgs e)
        {
            throw new PlatformNotSupportedException("Microsoft WebSocket client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Send a message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public override void SendMessage(string message)
        {
            throw new PlatformNotSupportedException("Microsoft WebSocket client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Set the Action to call when the connection has been established.
        /// </summary>
        /// <returns></returns>
        protected override Task OnConnectedAsync()
        {
            throw new PlatformNotSupportedException("Microsoft WebSocket client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <returns></returns>
        protected override Task OnDisconnectedAsync(SocketCloseEventArgs e)
        {
            throw new PlatformNotSupportedException("Microsoft WebSocket client is not supported on this platform. You need to target .NETFX, .NET Standard 1.3, or provide a WebSocket implementation for this platform.");
        }

        /// <summary>
        /// Creates a new instance of <see cref="WebSocketClient"/>.
        /// </summary>
        /// <param name="proxy">Proxy to use for this client instance.</param>
        /// <returns>An instance of <see cref="WebSocketClient"/>.</returns>
        public static BaseWebSocketClient CreateNew(IWebProxy proxy)
            => new WebSocketClient(proxy);

        #region Events
#pragma warning disable 649
        /// <summary>
        /// Triggered when the client connects successfully.
        /// </summary>
        public override event AsyncEventHandler OnConnect
        {
            add { this._on_connect.Register(value); }
            remove { this._on_connect.Unregister(value); }
        }
        private AsyncEvent _on_connect;

        /// <summary>
        /// Triggered when the client is disconnected.
        /// </summary>
        public override event AsyncEventHandler<SocketCloseEventArgs> OnDisconnect
        {
            add { this._on_disconnect.Register(value); }
            remove { this._on_disconnect.Unregister(value); }
        }
        private AsyncEvent<SocketCloseEventArgs> _on_disconnect;

        /// <summary>
        /// Triggered when the client receives a message from the remote party.
        /// </summary>
        public override event AsyncEventHandler<SocketMessageEventArgs> OnMessage
        {
            add { this._on_message.Register(value); }
            remove { this._on_message.Unregister(value); }
        }
        private AsyncEvent<SocketMessageEventArgs> _on_message;

        /// <summary>
        /// Triggered when an error occurs in the client.
        /// </summary>
        public override event AsyncEventHandler<SocketErrorEventArgs> OnError
        {
            add { this._on_error.Register(value); }
            remove { this._on_error.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _on_error;
#pragma warning restore 649
        #endregion
    }
}