using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus
{
    public class PresenceUpdateEventArgs : DiscordEventArgs
    {
        [JsonProperty("user")]
        internal DiscordUser InternalUser { get; set; }

        [JsonIgnore]
        public DiscordMember Member => this.Client._guilds[this.GuildId].Members.FirstOrDefault(xm => xm.Id == this.InternalUser.Id);

        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        public Game Game { get; internal set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; internal set; }

        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong GuildId { get; set; }

        [JsonIgnore]
        public DiscordGuild Guild => this.Client._guilds[this.GuildId];

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        internal IReadOnlyList<ulong> RoleIds { get; set; }

        [JsonIgnore]
        public IEnumerable<DiscordRole> Roles => this.RoleIds.Select(xid => this.Guild.Roles.FirstOrDefault(xr => xr.Id == xid));

        [JsonIgnore]
        public DiscordPresence PresenceBefore { get; internal set; }

        public PresenceUpdateEventArgs() : base(null) { }
        public PresenceUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
