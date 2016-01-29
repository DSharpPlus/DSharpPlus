using System;

namespace DiscordSharp
{
    public class DiscordDebugMessagesEventArgs : EventArgs
    {
        public string message { get; internal set; }
    }
}