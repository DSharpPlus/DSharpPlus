using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public abstract class BaseWebSocketClient
    {
        public abstract event AsyncEventHandler OnConnect;
        public abstract event AsyncEventHandler OnDisconnect;
        public abstract event AsyncEventHandler<WebSocketMessageEventArgs> OnMessage;
        internal static Type ClientType { get; set; } = typeof(WebSocketClient);

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <returns></returns>
        public static BaseWebSocketClient Create()
        {
            return (BaseWebSocketClient)Activator.CreateInstance(ClientType);
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <returns></returns>
        public abstract Task<WebSocketClient> ConnectAsync(string uri);

        /// <summary>
        /// Set the Action to call when the connection has been established.
        /// </summary>
        /// <returns></returns>
        public abstract Task<WebSocketClient> OnConnectAsync();

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <returns></returns>
        public abstract Task<WebSocketClient> OnDisconnectAsync();

        /// <summary>
        /// Send a message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public abstract void SendMessage(string message);
        
        public abstract Task InternalDisconnectAsync();
    }
}
