using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Net.WebSocket
{
    /// <summary>
    /// Creates an instance of a WebSocket client implementation.
    /// </summary>
    /// <param name="proxy">Proxy settings to use for the new WebSocket client instance.</param>
    /// <returns>Constructed WebSocket client implementation.</returns>
    public delegate IWebSocketClient WebSocketClientFactoryDelegate(IWebProxy proxy);

    /// <summary>
    /// Represents a base abstraction for all WebSocket client implementations.
    /// </summary>
    public interface IWebSocketClient : IDisposable
    {
        /// <summary>
        /// Gets the proxy settings for this client.
        /// </summary>
        IWebProxy Proxy { get; }

        /// <summary>
        /// Gets the collection of default headers to send when connecting to the remote endpoint.
        /// </summary>
        IReadOnlyDictionary<string, string> DefaultHeaders { get; }

        /// <summary>
        /// Connects to a specified remote WebSocket endpoint.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket endpoint.</param>
        /// <returns></returns>
        Task ConnectAsync(Uri uri);

        /// <summary>
        /// Disconnects the WebSocket connection.
        /// </summary>
        /// <returns></returns>
        Task DisconnectAsync(int code = 1000, string message = "");

        /// <summary>
        /// Send a message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        Task SendMessageAsync(string message);

        /// <summary>
        /// Adds a header to the default header collection.
        /// </summary>
        /// <param name="name">Name of the header to add.</param>
        /// <param name="value">Value of the header to add.</param>
        /// <returns>Whether the operation succeeded.</returns>
        bool AddDefaultHeader(string name, string value);

        /// <summary>
        /// Removes a header from the default header collection.
        /// </summary>
        /// <param name="name">Name of the header to remove.</param>
        /// <returns>Whether the operation succeeded.</returns>
        bool RemoveDefaultHeader(string name);

        /// <summary>
        /// Triggered when the client connects successfully.
        /// </summary>
        event AsyncEventHandler<IWebSocketClient, SocketEventArgs> Connected;

        /// <summary>
        /// Triggered when the client is disconnected.
        /// </summary>
        event AsyncEventHandler<IWebSocketClient, SocketCloseEventArgs> Disconnected;

        /// <summary>
        /// Triggered when the client receives a message from the remote party.
        /// </summary>
        event AsyncEventHandler<IWebSocketClient, SocketMessageEventArgs> MessageReceived;

        /// <summary>
        /// Triggered when an error occurs in the client.
        /// </summary>
        event AsyncEventHandler<IWebSocketClient, SocketErrorEventArgs> ExceptionThrown;
    }
}
