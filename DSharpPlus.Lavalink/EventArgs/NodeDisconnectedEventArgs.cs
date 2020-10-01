using Emzi0767.Utilities;

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

        internal NodeDisconnectedEventArgs(LavalinkNodeConnection node)
        {
            this.LavalinkNode = node;
        }
    }
}
