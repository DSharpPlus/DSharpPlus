using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildMemberUpdateEventArgs : EventArgs
    {
        public ulong GuildID;
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public List<ulong> Roles;
        public string NickName;
        public DiscordUser User;
    }
}
