using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordGuildScheduledEventUser
    {
        /// <summary>
        /// The scheduled event id which the user subscribed to.
        /// </summary>
        [JsonProperty("guild_scheduled_event_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildScheduledEventId { get; init; } = null!;

        /// <summary>
        /// The user which subscribed to an event.
        /// </summary>
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser User { get; init; } = null!;

        /// <summary>
        /// The guild member data for this user for the guild which this event belongs to, if any.
        /// </summary>
        [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordGuildMember> Member { get; init; }
    }
}
