using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus
{
    public class PresenceUpdateEventArgs : DiscordEventArgs
    {
        [JsonProperty("user")]
        internal DiscordUser InternalUser { get; set; }

        /// <summary>
        /// Member whose presence was updated
        /// </summary>
        [JsonIgnore]
        public DiscordMember Member => this.Client._guilds[this.GuildId].Members.FirstOrDefault(xm => xm.Id == this.InternalUser.Id);

        /// <summary>
        /// Member's new game
        /// </summary>
        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        public Game Game { get; internal set; }

        /// <summary>
        /// Member's status
        /// </summary>
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; internal set; }

        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong GuildId { get; set; }

        /// <summary>
        /// Guild this member belongs to
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild => this.Client._guilds[this.GuildId];

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        internal IReadOnlyList<ulong> RoleIds { get; set; }

        /// <summary>
        /// Roles this member has
        /// </summary>
        [JsonIgnore]
        public IEnumerable<DiscordRole> Roles => this.RoleIds.Select(xid => this.Guild.Roles.FirstOrDefault(xr => xr.Id == xid));

        /// <summary>
        /// Member's old presence
        /// </summary>
        [JsonIgnore]
        public DiscordPresence PresenceBefore { get; internal set; }

        public PresenceUpdateEventArgs() : base(null) { }
        public PresenceUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
