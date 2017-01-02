using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class DiscordPresence
    {
        [JsonProperty("user")]
        internal DiscordUser InternalUser;

        public ulong UserID => InternalUser == null ? 0 : InternalUser.ID;

        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        internal JObject InternalGame;

        public string Game => (InternalGame == null) ? "" : InternalGame["name"].ToString();

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status;

        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        ulong GuildID;
    }
}
