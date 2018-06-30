using System;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.SocketClosed"/> event.
    /// </summary>
    public class SocketCloseEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the close code sent by remote host.
        /// </summary>
        public int CloseCode { get; set; }

        /// <summary>
        /// Gets the close message sent by remote host.
        /// </summary>
        public string CloseMessage { get; set; }
        
        public SocketCloseEventArgs(DiscordClient client) : base(client) { }
    }

    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.SocketErrored"/> event.
    /// </summary>
    public class SocketErrorEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the exception thrown by websocket client.
        /// </summary>
        public Exception Exception { get; set; }

        public SocketErrorEventArgs(DiscordClient client) : base(client) { }
    }
}
