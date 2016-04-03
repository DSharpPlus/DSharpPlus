using System;
using DiscordSharp.Objects;
namespace DiscordSharp
{
    public class DiscordSocketClosedEventArgs : EventArgs
    {
        public int Code { get; internal set; }
        public string Reason { get; internal set; }
        public bool WasClean { get; internal set; }

    }
}