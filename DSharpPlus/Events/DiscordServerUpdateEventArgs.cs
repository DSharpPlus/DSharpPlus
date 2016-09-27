using System;
using DSharpPlus.Objects;

namespace DSharpPlus.Events
{
    public class DiscordServerUpdateEventArgs : EventArgs
    {
        public DiscordServer NewServer { get; set; }
        public DiscordServer OldServer { get; set; }
    }
}
