using System;

namespace DSharpPlus
{
    public class GuildUpdateEventArgs : EventArgs
    {
        public DiscordGuild Guild { get; internal set; }
    }
}
