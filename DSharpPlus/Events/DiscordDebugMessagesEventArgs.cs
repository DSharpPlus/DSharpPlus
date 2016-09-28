using System;

namespace DSharpPlus
{
    public class DiscordDebugMessagesEventArgs : EventArgs
    {
        public string Message { get; internal set; }
    }
}