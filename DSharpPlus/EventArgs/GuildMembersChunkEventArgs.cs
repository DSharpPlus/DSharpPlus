using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildMembersChunkEventArgs : EventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => DiscordClient.InternalGetGuildAsync(GuildID).Result;
        public IReadOnlyList<DiscordMember> Members { get; internal set; }
    }
}
