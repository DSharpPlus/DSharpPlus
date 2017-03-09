using System;

namespace DSharpPlus
{
    public class GuildMemberAddEventArgs : EventArgs
    {
        public DiscordMember Member { get; internal set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
    }
}
