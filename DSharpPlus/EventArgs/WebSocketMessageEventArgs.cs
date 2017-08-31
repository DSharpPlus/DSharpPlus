namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for raw socket message events.
    /// </summary>
    public class SocketMessageEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the received message.
        /// </summary>
        public string Message { get; internal set; }

        internal SocketMessageEventArgs() { }
    }
}
