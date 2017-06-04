using System;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public abstract class BaseWebSocketClient
    {
        public abstract event AsyncEventHandler OnConnect;
        public abstract event AsyncEventHandler<SocketDisconnectEventArgs> OnDisconnect;
        public abstract event AsyncEventHandler<WebSocketMessageEventArgs> OnMessage;
        internal static Type ClientType { get; set; } = typeof(WebSocketClient);

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <returns></returns>
        public static BaseWebSocketClient Create()
        {
            return (BaseWebSocketClient)Activator.CreateInstance(ClientType);
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <returns></returns>
        public abstract Task<BaseWebSocketClient> ConnectAsync(string uri);

        /// <summary>
        /// Set the Action to call when the connection has been established.
        /// </summary>
        /// <returns></returns>
        public abstract Task<BaseWebSocketClient> OnConnectAsync();

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <returns></returns>
        public abstract Task<BaseWebSocketClient> OnDisconnectAsync(SocketDisconnectEventArgs e);

        /// <summary>
        /// Send a message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public abstract void SendMessage(string message);
        
        public abstract Task InternalDisconnectAsync(SocketDisconnectEventArgs e);
    }
}
