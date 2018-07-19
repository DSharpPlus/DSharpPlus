using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Net.WebSocket
{
    /// <summary>
    /// Creates an instance of a WebSocket client implementation.
    /// </summary>
    /// <param name="proxy">Proxy settings to use for the new WebSocket client instance.</param>
    /// <returns>Constructed WebSocket client implementation.</returns>
    public delegate BaseWebSocketClient WebSocketClientFactoryDelegate(IWebProxy proxy);

    /// <summary>
    /// Represents a base abstraction for all WebSocket client implementations.
    /// </summary>
    public abstract class BaseWebSocketClient : IDisposable
    {
        /// <summary>
        /// Gets the ZLib stream suffix (0xFFFF) to detect stream-compressed messages.
        /// </summary>
        protected const ushort ZLIB_STREAM_SUFFIX = 0xFFFF;

        /// <summary>
        /// Gets the proxy settings for this client.
        /// </summary>
        public IWebProxy Proxy { get; }

        /// <summary>
        /// Gets the stream decompressor for stream compression.
        /// </summary>
        protected DeflateStream StreamDecompressor { get; set; }

        /// <summary>
        /// Gets the target stream for the stream decompressor.
        /// </summary>
        protected MemoryStream DecompressedStream { get; set; }

        /// <summary>
        /// Gets the source stream for the stream decompressor.
        /// </summary>
        protected MemoryStream CompressedStream { get; set; }

        /// <summary>
        /// Creates a new WebSocket client.
        /// </summary>
        public BaseWebSocketClient(IWebProxy proxy)
        {
            this.Proxy = proxy;
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <param name="customHeaders">Custom headers to send with the request.</param>
        /// <returns></returns>
        public abstract Task ConnectAsync(Uri uri, IReadOnlyDictionary<string, string> customHeaders = null);

        /// <summary>
        /// Disconnects the WebSocket connection.
        /// </summary>
        /// <param name="e">Disconect event arguments.</param>
        /// <returns></returns>
        public abstract Task DisconnectAsync(SocketCloseEventArgs e);

        /// <summary>
        /// Send a message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public abstract void SendMessage(string message);

        /// <summary>
        /// Set the Action to call when the connection has been established.
        /// </summary>
        /// <returns></returns>
        protected abstract Task OnConnectedAsync();

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <returns></returns>
        protected abstract Task OnDisconnectedAsync(SocketCloseEventArgs e);

        /// <summary>
        /// Disposes this socket client.
        /// </summary>
        public virtual void Dispose()
        {
            this.StreamDecompressor?.Dispose();
            this.CompressedStream?.Dispose();
            this.DecompressedStream?.Dispose();
        }

        /// <summary>
        /// Triggered when the client connects successfully.
        /// </summary>
        public abstract event AsyncEventHandler OnConnect;

        /// <summary>
        /// Triggered when the client is disconnected.
        /// </summary>
        public abstract event AsyncEventHandler<SocketCloseEventArgs> OnDisconnect;

        /// <summary>
        /// Triggered when the client receives a message from the remote party.
        /// </summary>
        public abstract event AsyncEventHandler<SocketMessageEventArgs> OnMessage;

        /// <summary>
        /// Triggered when an error occurs in the client.
        /// </summary>
        public abstract event AsyncEventHandler<SocketErrorEventArgs> OnError;
    }
}
