using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordGuildScheduledEventUser
    {
        /// <summary>
        /// The scheduled event id which the user subscribed to.
        /// </summary>
        [JsonPropertyName("guild_scheduled_event_id")]
        public DiscordSnowflake GuildScheduledEventId { get; init; } = null!;

        /// <summary>
        /// The user which subscribed to an event.
        /// </summary>
        [JsonPropertyName("user")]
        public DiscordUser User { get; init; } = null!;

        /// <summary>
        /// The guild member data for this user for the guild which this event belongs to, if any.
        /// </summary>
        [JsonPropertyName("member")]
        public Optional<DiscordGuildMember> Member { get; init; }
    }
}
