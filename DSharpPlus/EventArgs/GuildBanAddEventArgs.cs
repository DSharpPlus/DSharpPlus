using System;

namespace DSharpPlus
{
    public class GuildBanAddEventArgs : EventArgs
    {
        public DiscordUser User;
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
    }
}
