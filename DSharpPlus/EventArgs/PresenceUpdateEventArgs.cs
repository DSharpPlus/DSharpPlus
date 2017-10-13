﻿using System.Collections.Generic;
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
        [JsonProperty("user")]
        internal DiscordUser InternalUser { get; set; }

        /// <summary>
        /// Gets the member whose presence was updated.
        /// </summary>
        [JsonIgnore]
        public DiscordMember Member => Client.Guilds[GuildId].Members.FirstOrDefault(xm => xm.Id == InternalUser.Id);

        /// <summary>
        /// Gets the member's new game.
        /// </summary>
        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        public TransportGame Game { get; internal set; }

        /// <summary>
        /// Gets the member's status.
        /// </summary>
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; internal set; }

        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong GuildId { get; set; }

        /// <summary>
        /// Gets the guild for which this event occured.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild => Client.Guilds[GuildId];

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        internal IReadOnlyList<ulong> RoleIds { get; set; }

        /// <summary>
        /// Gets the roles this member has.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<DiscordRole> Roles => RoleIds.Select(xid => Guild.Roles.FirstOrDefault(xr => xr.Id == xid));

        /// <summary>
        /// Gets the member's old presence.
        /// </summary>
        [JsonIgnore]
        public DiscordPresence PresenceBefore { get; internal set; }

        internal PresenceUpdateEventArgs() : base(null) { }
        internal PresenceUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
