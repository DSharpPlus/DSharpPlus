using System;
using SharpCord.Objects;
namespace SharpCord
{
    public class DiscordDebugMessagesEventArgs : EventArgs
    {
        public string Message { get; internal set; }
    }
}