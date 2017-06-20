using System;

namespace DSharpPlus
{
    public class ClientErrorEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Exception thrown by client
        /// </summary>
        public Exception Exception { get; internal set; }
        /// <summary>
        /// Event that threw the exception
        /// </summary>
        public string EventName { get; internal set; }

        public ClientErrorEventArgs(DiscordClient client) : base(client) { }
    }
}
