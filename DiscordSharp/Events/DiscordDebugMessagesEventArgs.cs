using System;
using DiscordSharp.Objects;
namespace DiscordSharp
{
    public class DiscordDebugMessagesEventArgs : EventArgs
    {
        public string message { get; internal set; }
    }
}