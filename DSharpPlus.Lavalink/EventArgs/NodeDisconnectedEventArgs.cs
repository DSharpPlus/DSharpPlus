using DSharpPlus.AsyncEvents;

namespace DSharpPlus.Lavalink.EventArgs
{
    /// <summary>
    /// Represents event arguments for Lavalink node disconnection.
    /// </summary>
    public sealed class NodeDisconnectedEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// Gets the node that was disconnected.
        /// </summary>
        public LavalinkNodeConnection LavalinkNode { get; }

        /// <summary>
        /// Gets whether disconnect was clean.
        /// </summary>
        public bool IsCleanClose { get; }

        internal NodeDisconnectedEventArgs(LavalinkNodeConnection node, bool isClean)
        {
            this.LavalinkNode = node;
            this.IsCleanClose = isClean;
        }
    }
}
