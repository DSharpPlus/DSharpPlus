using System.Collections.Generic;

namespace DSharpPlus
{
    public class GuildMemberUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Guild that just got one of its members updated
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        /// <summary>
        /// Member's new roles
        /// </summary>
        public IReadOnlyList<DiscordRole> RolesAfter { get; internal set; }
        /// <summary>
        /// Member's old roles
        /// </summary>
        public IReadOnlyList<DiscordRole> RolesBefore { get; internal set; }

        /// <summary>
        /// Member's new nickname
        /// </summary>
        public string NicknameAfter { get; internal set; }
        /// <summary>
        /// Member's old nickname
        /// </summary>
        public string NicknameBefore { get; internal set; }

        /// <summary>
        /// Member that got updated
        /// </summary>
        public DiscordMember Member { get; internal set; }

        public GuildMemberUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
