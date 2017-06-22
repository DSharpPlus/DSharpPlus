using System;

namespace DSharpPlus
{
    public class SocketMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Received Message
        /// </summary>
        public string Message { get; internal set; }
    }
}
