using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildMemberUpdateEventArgs : DiscordEventArgs
    {
        public DiscordGuild Guild { get; internal set; }

        public IReadOnlyList<DiscordRole> RolesAfter { get; internal set; }
        public IReadOnlyList<DiscordRole> RolesBefore { get; internal set; }

        public string NicknameAfter { get; internal set; }
        public string NicknameBefore { get; internal set; }

        public DiscordMember Member { get; internal set; }

        public GuildMemberUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
