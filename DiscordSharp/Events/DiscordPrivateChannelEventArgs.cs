using System;

namespace DiscordSharp
{
    public class DiscordPrivateChannelEventArgs : EventArgs
    { 
        public DiscordChannelCreateType ChannelType { get; set; }
        public DiscordPrivateChannel ChannelCreated { get; set; }
    }
}