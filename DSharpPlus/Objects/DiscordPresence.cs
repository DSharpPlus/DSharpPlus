using System.Linq;
using DSharpPlus.Objects.Transport;
using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordPresence
    {
        [JsonIgnore]
        internal DiscordClient Discord { get; set; }

        [JsonProperty("user")]
        internal TransportUser InternalUser { get; set; }

        [JsonIgnore]
        public DiscordUser User => this.Guild != null ? this.Guild._members.FirstOrDefault(xm => xm.Id == this.InternalUser.Id) : this.Discord.InternalGetCachedUser(this.InternalUser.Id) ?? new DiscordUser(this.InternalUser) { Discord = this.Discord };

        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        public Game Game { get; internal set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; internal set; }

        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong GuildId { get; set; }

        [JsonIgnore]
        public DiscordGuild Guild => this.GuildId != 0 ? this.Discord._guilds[this.GuildId] : null;
    }
}
