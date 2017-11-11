using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.PresenceUpdated"/> event.
    /// </summary>
    public class PresenceUpdateEventArgs : DiscordEventArgs
    {
        internal DiscordUser InternalUser { get; set; }

        /// <summary>
        /// Gets the member whose presence was updated.
        /// </summary>
        public DiscordMember Member 
            => this.Client.Guilds[this.GuildId].Members.FirstOrDefault(xm => xm.Id == this.InternalUser.Id);

        /// <summary>
        /// Gets the member's new game.
        /// </summary>
        public DiscordActivity Activity { get; internal set; }

        /// <summary>
        /// Gets the member's status.
        /// </summary>
        public UserStatus Status { get; internal set; }
        
        internal ulong GuildId { get; set; }

        /// <summary>
        /// Gets the guild for which this event occured.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild 
            => this.Client.Guilds[this.GuildId];
        
        internal IReadOnlyList<ulong> RoleIds { get; set; }

        /// <summary>
        /// Gets the roles this member has.
        /// </summary>
        public IEnumerable<DiscordRole> Roles 
            => this.RoleIds.Select(xid => this.Guild.Roles.FirstOrDefault(xr => xr.Id == xid));

        /// <summary>
        /// Gets the member's old presence.
        /// </summary>
        public DiscordPresence PresenceBefore { get; internal set; }

        internal PresenceUpdateEventArgs() : base(null) { }
        internal PresenceUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
