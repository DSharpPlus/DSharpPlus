using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus
{
    public class DiscordPresence
    {
        [JsonProperty("user")]
        internal DiscordUser InternalUser { get; set; }

        public DiscordUser User => this.User.Discord.InternalGetCachedUser(UserID);

        public ulong UserID => InternalUser == null ? 0 : InternalUser.Id;

        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        internal JObject InternalGame;

        public string Game => (InternalGame == null) ? "" : InternalGame["name"].ToString();

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status;

        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        private ulong GuildID { get; set; }
    }
}
