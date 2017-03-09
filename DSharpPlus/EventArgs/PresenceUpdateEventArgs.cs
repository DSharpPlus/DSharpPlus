using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class PresenceUpdateEventArgs : EventArgs
    {
        public DiscordUser User => DiscordClient._guilds[GuildID].Members.Find(x => x.User.ID == UserID)?.User;

        [JsonProperty("user")]
        internal DiscordUser InternalUser { get; set; }

        public ulong UserID => InternalUser == null ? 0 : InternalUser.ID;

        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        internal JObject InternalGame { get; set; }

        public string Game => (InternalGame == null) ? "" : InternalGame["name"].ToString();

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; internal set; }

        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong GuildID { get; internal set; }

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyCollection<ulong> RoleIDs { get; internal set; }
    }
}
