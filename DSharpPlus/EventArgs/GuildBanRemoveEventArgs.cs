using System;

namespace DSharpPlus
{
    public class GuildBanRemoveEventArgs : EventArgs
    {
        public DiscordUser User;
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
    }
}
