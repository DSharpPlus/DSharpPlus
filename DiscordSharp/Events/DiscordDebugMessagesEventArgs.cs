using System;
using DiscordSharp.Objects;
namespace DiscordSharp
{
    public class DiscordDebugMessagesEventArgs : EventArgs
    {
        public string Message { get; internal set; }
    }
}