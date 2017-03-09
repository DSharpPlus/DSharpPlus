using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildMembersChunkEventArgs : EventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public IReadOnlyCollection<DiscordMember> Members { get; internal set; }
    }
}
