using System.Collections.Generic;
using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class GuildMembersChunkEventArgs : System.EventArgs
    {
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public List<DiscordMember> Members;
    }
}
