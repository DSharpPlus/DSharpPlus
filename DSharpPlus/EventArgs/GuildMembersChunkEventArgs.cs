using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildMembersChunkEventArgs : EventArgs
    {
        internal DiscordClient Discord { get; set; }
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Discord._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
        public IReadOnlyList<DiscordMember> Members { get; internal set; }
    }
}
