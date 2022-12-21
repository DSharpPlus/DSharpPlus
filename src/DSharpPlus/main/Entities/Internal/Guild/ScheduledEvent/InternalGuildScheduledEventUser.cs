using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record InternalGuildScheduledEventUser
    {
        /// <summary>
        /// The scheduled event id which the user subscribed to.
        /// </summary>
        [JsonPropertyName("guild_scheduled_event_id")]
        public InternalSnowflake GuildScheduledEventId { get; init; } = null!;

        /// <summary>
        /// The user which subscribed to an event.
        /// </summary>
        [JsonPropertyName("user")]
        public InternalUser User { get; init; } = null!;

        /// <summary>
        /// The guild member data for this user for the guild which this event belongs to, if any.
        /// </summary>
        [JsonPropertyName("member")]
        public Optional<InternalGuildMember> Member { get; init; }
    }
}
