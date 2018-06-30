using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a user presence.
    /// </summary>
    public class DiscordPresence : PropertyChangedBase
    {
        private string _internalStatus;

        [JsonIgnore]
        internal DiscordClient Discord { get; set; }

        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        internal TransportUser InternalUser { get; set; }

        /// <summary>
        /// Gets the user that owns this presence.
        /// </summary>
        [JsonIgnore]
        public DiscordUser User
            => Discord.InternalGetCachedUser(InternalUser.Id);

        /// <summary>
        /// Gets the game this user is playing.
        /// </summary>
        [JsonIgnore]
        public DiscordActivity Activity { get; internal set; }

        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        internal TransportActivity RawActivity { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        internal string InternalStatus
        {
            get => _internalStatus;
            set
            {
                OnPropertySet(ref _internalStatus, value);
                InvokePropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// Gets this user's status.
        /// </summary>
        [JsonIgnore]
        public UserStatus Status
        {
            get
            {
                switch (InternalStatus?.ToLowerInvariant())
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
            => GuildId != 0 ? Discord._guilds[GuildId] : null;

        internal DiscordPresence() { }

        internal DiscordPresence(DiscordPresence other)
        {
            Discord = other.Discord;
            Activity = other.Activity;
            RawActivity = other.RawActivity;
            InternalStatus = other.InternalStatus;
            InternalUser = new TransportUser(other.InternalUser);
        }
    }
}
