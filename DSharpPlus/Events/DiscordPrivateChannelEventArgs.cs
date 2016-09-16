using System;
using DSharpPlus.Objects;
namespace DSharpPlus
{
    public class DiscordPrivateChannelEventArgs : EventArgs
    { 
        public DiscordChannelCreateType ChannelType { get; set; }
        public DiscordPrivateChannel ChannelCreated { get; set; }
    }
}