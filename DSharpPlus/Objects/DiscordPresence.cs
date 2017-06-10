using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordPresence
    {
        [JsonIgnore]
        internal DiscordClient Discord { get; set; }

        [JsonProperty("user")]
        internal DiscordUser InternalUser { get; set; }

        [JsonIgnore]
        public DiscordUser User => this.Discord.InternalGetCachedUser(this.InternalUser.Id);

        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        public Game Game { get; internal set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; internal set; }

        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        private ulong GuildId { get; set; }

        [JsonIgnore]
        public DiscordGuild Guild => this.Discord._guilds[this.GuildId];
    }
}
