using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildMemberUpdateEventArgs : EventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.User.Discord._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
        public IReadOnlyList<ulong> Roles { get; internal set; }
        public IReadOnlyList<ulong> RolesBefore { get; internal set; }
        public string NickName { get; internal set; }
        public string NickNameBefore { get; internal set; }
        public DiscordUser User { get; internal set; }
    }
}
