using System;

namespace DSharpPlus
{
    public class GuildMemberAddEventArgs : EventArgs
    {
        public DiscordMember Member;
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
    }
}
