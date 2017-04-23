using System;

namespace DSharpPlus
{
    public class DmChannelDeleteEventArgs : EventArgs
    {
        public DiscordDmChannel Channel { get; internal set; }
    }
}
