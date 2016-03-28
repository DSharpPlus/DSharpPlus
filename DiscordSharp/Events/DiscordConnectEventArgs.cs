using System;
using DiscordSharp.Objects;
namespace DiscordSharp
{
    public class DiscordConnectEventArgs : EventArgs
    {
        public DiscordMember User { get; internal set; }
    }
}