using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a user presence.
    /// </summary>
    public class DiscordPresence
    {
        [JsonIgnore]
        internal DiscordClient Discord { get; set; }

        [JsonProperty("user")]
        internal TransportUser InternalUser { get; set; }

        /// <summary>
        /// Gets the user that owns this presence.
        /// </summary>
        [JsonIgnore]
        public DiscordUser User 
            => this.Discord.InternalGetCachedUser(this.InternalUser.Id);

        /// <summary>
        /// Gets the game this user is playing.
        /// </summary>
        [JsonIgnore]
        public DiscordActivity Activity { get; internal set; }

        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        internal TransportActivity RawActivity { get; set; }
        
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        internal string InternalStatus { get; set; }

        /// <summary>
        /// Gets this user's status.
        /// </summary>
        [JsonIgnore]
        public UserStatus Status
        {
            get
            {
                switch (this.InternalStatus.ToLowerInvariant())
                {
                    case "online":
                        return UserStatus.Online;

                    case "idle":
                        return UserStatus.Idle;

                    case "dnd":
                        return UserStatus.DoNotDisturb;

                    case "invisible":
                        return UserStatus.Invisible;

                    case "offline":
                    default:
                        return UserStatus.Offline;
                }
            }
        }

        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong GuildId { get; set; }

        /// <summary>
        /// Gets the guild for which this presence was set.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild 
            => this.GuildId != 0 ? this.Discord._guilds[this.GuildId] : null;

        internal DiscordPresence() { }

        internal DiscordPresence(DiscordPresence other)
        {
            this.Discord = other.Discord;
            this.Activity = other.Activity;
            this.RawActivity = other.RawActivity;
            this.InternalStatus = other.InternalStatus;
            this.InternalUser = new TransportUser(other.InternalUser);
        }
    }
}
