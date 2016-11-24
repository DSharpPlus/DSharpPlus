using DSharpPlus.Objects;
using System;

namespace DSharpPlus.Events
{
    public class DiscordChannelDeleteEventArgs : EventArgs
    {
        public DiscordChannel ChannelDeleted { get; internal set; }
    }

    public class DiscordPrivateChannelDeleteEventArgs : EventArgs
    {
        public DiscordPrivateChannel PrivateChannelDeleted { get; internal set; }
    }
}
