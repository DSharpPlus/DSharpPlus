using System;

namespace DSharpPlus
{
    public class GuildMemberRemoveEventArgs : EventArgs
    {
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public DiscordUser User;
    }
}
