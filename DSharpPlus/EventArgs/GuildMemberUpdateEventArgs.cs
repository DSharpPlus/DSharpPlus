using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.GuildMemberUpdated"/> event.
    /// </summary>
    public class GuildMemberUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the guild in which the update occurred.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        /// <summary>
        /// Gets a collection containing post-update roles.
        /// </summary>
        public IReadOnlyList<DiscordRole> RolesAfter { get; internal set; }

        /// <summary>
        /// Gets a collection containing pre-update roles.
        /// </summary>
        public IReadOnlyList<DiscordRole> RolesBefore { get; internal set; }

        /// <summary>
        /// Gets the member's new nickname.
        /// </summary>
        public string NicknameAfter { get; internal set; }

        /// <summary>
        /// Gets the member's old nickname.
        /// </summary>
        public string NicknameBefore { get; internal set; }

        /// <summary>
        /// Gets the member that was updated.
        /// </summary>
        public DiscordMember Member { get; internal set; }

        internal GuildMemberUpdateEventArgs() : base() { }
    }
}
