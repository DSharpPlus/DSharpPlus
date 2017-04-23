using System;

namespace DSharpPlus
{
    public class DmChannelCreateEventArgs : EventArgs
    {
        public DiscordDmChannel Channel { get; internal set; }
    }
}
