using System;

namespace DSharpPlus
{
    public class DMChannelDeleteEventArgs : EventArgs
    {
        public DiscordDMChannel Channel { get; internal set; }
    }
}
