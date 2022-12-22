using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalGuildScheduledEventUser
{
    /// <summary>
    /// The scheduled event id which the user subscribed to.
    /// </summary>
    [JsonPropertyName("guild_scheduled_event_id")]
    public required Snowflake GuildScheduledEventId { get; init; }

    /// <summary>
    /// The user which subscribed to an event.
    /// </summary>
    [JsonPropertyName("user")]
    public required InternalUser User { get; init; } 

    /// <summary>
    /// The guild member data for this user for the guild which this event belongs to, if any.
    /// </summary>
    [JsonPropertyName("member")]
    public Optional<InternalGuildMember> Member { get; init; }
}
