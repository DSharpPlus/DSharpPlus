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
        public override event AsyncEventHandler Connected
        {
            add => this._connected.Register(value);
            remove => this._connected.Unregister(value);
        }
        private AsyncEvent _connected;

        /// <summary>
        /// Triggered when the client is disconnected.
        /// </summary>
        public override event AsyncEventHandler<SocketCloseEventArgs> Disconnected
        {
            add => this._disconnected.Register(value);
            remove => this._disconnected.Unregister(value);
        }
        private AsyncEvent<SocketCloseEventArgs> _disconnected;

        /// <summary>
        /// Triggered when the client receives a message from the remote party.
        /// </summary>
        public override event AsyncEventHandler<SocketMessageEventArgs> MessageReceived
        {
            add => this._messageReceived.Register(value);
            remove => this._messageReceived.Unregister(value);
        }
        private AsyncEvent<SocketMessageEventArgs> _messageReceived;

        /// <summary>
        /// Triggered when an error occurs in the client.
        /// </summary>
        public override event AsyncEventHandler<SocketErrorEventArgs> Errored
        {
            add => this._errored.Register(value);
            remove => this._errored.Unregister(value);
        }
        private AsyncEvent<SocketErrorEventArgs> _errored;
#pragma warning restore 649
        #endregion
    }
}