using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DSharpPlus
{
    public class PresenceUpdateEventArgs : EventArgs
    {
        public DiscordUser User => DiscordClient._guilds[GuildID].Members.Find(x => x.User.ID == UserID).User;

        [JsonProperty("user")]
        internal DiscordUser InternalUser;

        public ulong UserID => InternalUser == null ? 0 : InternalUser.ID;

        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        internal JObject InternalGame;

        public string Game => (InternalGame == null) ? "" : InternalGame["name"].ToString();

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status;

        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong GuildID;

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public List<ulong> RoleIDs;
    }
}
