using DiscordSharp.Objects;
using System;

namespace DiscordSharp
{
    public enum DiscordChannelCreateType
    {
        PRIVATE, CHANNEL
    }
    public class DiscordChannelCreateEventArgs : EventArgs
    {
        public DiscordChannelCreateType ChannelType { get; set; }
        public DiscordChannel ChannelCreated { get; set; }
    }
}