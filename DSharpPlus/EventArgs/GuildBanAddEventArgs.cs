using System;

namespace DSharpPlus
{
    public class GuildBanAddEventArgs : EventArgs
    {
        public DiscordUser User { get; internal set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
    }
}
