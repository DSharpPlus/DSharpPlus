using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Sockets
{
    public class SocketMessageEventArgs : EventArgs
    {
        public string Message { get; internal set; }
    }

    public class SocketClosedEventArgs : EventArgs
    {
        public string Reason { get; internal set; }
        public int Code { get; internal set; }
        public bool WasClean { get; internal set; }
    }

    public class SocketErrorEventArgs : EventArgs
    {
        public Exception Exception { get; internal set; }
        public string Message { get; internal set; }
    }


    /// <summary>
    /// Provides an interface for all DiscordSharp websockets to implement for multiple backends without heavy editing.
    /// </summary>
    public interface IDiscordWebSocket
    {
        /// <summary>
        /// The URL to connect to.
        /// </summary>
        string URL { get; set; }

        /// <summary>
        /// Whether or not the socket is alive.
        /// </summary>
        bool IsAlive { get; }

        void Connect();
        void Close();
        void Send(string data);
        void Send(byte[] data);

        /// <summary>
        /// Occurs when the websocket receives a message.
        /// </summary>
        event EventHandler<SocketMessageEventArgs> MessageReceived;

        /// <summary>
        /// Occurs when the socket is closed.
        /// </summary>
        event EventHandler<SocketClosedEventArgs> SocketClosed;

        /// <summary>
        /// Occurs when the socket is opened.
        /// </summary>
        event EventHandler<EventArgs> SocketOpened;

        /// <summary>
        /// Occurs when the socket receives an error.
        /// </summary>
        event EventHandler<SocketErrorEventArgs> SocketError;
    }
}
