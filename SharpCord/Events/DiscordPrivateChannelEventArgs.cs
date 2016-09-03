using System;
using SharpCord.Objects;
namespace SharpCord
{
    public class DiscordPrivateChannelEventArgs : EventArgs
    { 
        public DiscordChannelCreateType ChannelType { get; set; }
        public DiscordPrivateChannel ChannelCreated { get; set; }
    }
}