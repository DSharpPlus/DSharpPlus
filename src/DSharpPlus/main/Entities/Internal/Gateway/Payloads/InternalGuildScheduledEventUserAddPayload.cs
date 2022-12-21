using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when a user has subscribed to a guild scheduled event.
/// </summary>
public sealed record InternalGuildScheduledEventUserAddPayload
{
    /// <summary>
    /// The id of the guild scheduled event.
    /// </summary>
    [JsonPropertyName("guild_scheduled_event_id")]
    public Snowflake GuildScheduledEventId { get; init; } = null!;

    /// <summary>
    /// The id of the user.
    /// </summary>
    [JsonPropertyName("user_id")]
    public Snowflake UserId { get; init; } = null!;

    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; init; } = null!;
}
