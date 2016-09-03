using System;
using SharpCord.Objects;
namespace SharpCord
{
    public class DiscordSocketClosedEventArgs : EventArgs
    {
        public int Code { get; internal set; }
        public string Reason { get; internal set; }
        public bool WasClean { get; internal set; }

    }
}