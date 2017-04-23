using System;

namespace DSharpPlus
{
    public class ClientErrorEventArgs : DiscordEventArgs
    {
        public Exception Exception { get; internal set; }
        public string EventName { get; internal set; }

        public ClientErrorEventArgs(DiscordClient client) : base(client) { }
    }
}
