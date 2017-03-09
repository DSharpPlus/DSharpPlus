using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildMemberUpdateEventArgs : EventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => DiscordClient.InternalGetGuild(GuildID).Result;
        public IReadOnlyCollection<ulong> Roles { get; internal set; }
        public string NickName { get; internal set; }
        public DiscordUser User { get; internal set; }
    }
}
