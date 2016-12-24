using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildMembersChunkEventArgs : EventArgs
    {
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public List<DiscordMember> Members;
    }
}
