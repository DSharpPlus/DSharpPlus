// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
