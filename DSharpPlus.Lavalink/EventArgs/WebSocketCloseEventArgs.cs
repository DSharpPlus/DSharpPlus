using DSharpPlus.AsyncEvents;

namespace DSharpPlus.Lavalink.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="LavalinkGuildConnection.DiscordWebSocketClosed"/> event.
    /// </summary>
    public sealed class WebSocketCloseEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// Gets the WebSocket close code.
        /// </summary>
        public int Code { get; }

        /// <summary>
        /// Gets the WebSocket close reason.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Gets whether the termination was initiated by the remote party (i.e. Discord).
        /// </summary>
        public bool Remote { get; }

        internal WebSocketCloseEventArgs(int code, string reason, bool remote)
        {
            this.Code = code;
            this.Reason = reason;
            this.Remote = remote;
        }
    }
}
