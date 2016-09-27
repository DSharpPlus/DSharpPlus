using System;
using DSharpPlus.Objects;

namespace DSharpPlus
{
    public class DiscordConnectEventArgs : EventArgs
    {
        public DiscordMember User { get; internal set; }
    }
}