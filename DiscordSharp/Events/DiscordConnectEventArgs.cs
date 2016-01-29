using System;

namespace DiscordSharp
{
    public class DiscordConnectEventArgs : EventArgs
    {
        public DiscordMember user { get; internal set; }
    }
}