using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildMemberUpdateEventArgs : DiscordEventArgs
    {
        public ulong GuildID { get; internal set; }
        public DiscordGuild Guild => this.Client.Guilds[GuildID];
        public IReadOnlyList<ulong> Roles { get; internal set; }
        public IReadOnlyList<ulong> RolesBefore { get; internal set; }
        public string NickName { get; internal set; }
        public string NickNameBefore { get; internal set; }
        public DiscordUser User { get; internal set; }

        public GuildMemberUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
