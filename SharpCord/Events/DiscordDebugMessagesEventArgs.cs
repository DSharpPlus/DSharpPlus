using System;
using DSharpPlus.Objects;
namespace DSharpPlus
{
    public class DiscordDebugMessagesEventArgs : EventArgs
    {
        public string Message { get; internal set; }
    }
}