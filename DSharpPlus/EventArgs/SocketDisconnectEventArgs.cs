namespace DSharpPlus
{
    /// <summary>
    /// Represents arguments for SocketDisconnect event.
    /// </summary>
    public class SocketDisconnectEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the close code sent by remote host.
        /// </summary>
        public int CloseCode { get; internal set; }

        /// <summary>
        /// Gets the close message sent by remote host.
        /// </summary>
        public string CloseMessage { get; internal set; }
        
        internal SocketDisconnectEventArgs(DiscordClient client) : base(client) { }
    }
}
