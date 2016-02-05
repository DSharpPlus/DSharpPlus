using System;
using DiscordSharp.Objects;
namespace DiscordSharp
{
    public class DiscordConnectEventArgs : EventArgs
    {
        public DiscordMember user { get; internal set; }
    }
}