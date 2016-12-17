using System.Collections.Generic;
using DSharpPlus.Objects;

namespace DSharpPlus.EventArgs
{
    public class GuildMemberUpdateEventArgs : System.EventArgs
    {
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public List<ulong> Roles;
        public string NickName;
        public DiscordUser User;
    }
}
