using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when a user has unsubscribed from a guild scheduled event.
/// </summary>
public sealed record InternalGuildScheduledEventUserRemovePayload
{
    /// <summary>
    /// The id of the guild scheduled event.
    /// </summary>
    [JsonPropertyName("guild_scheduled_event_id")]
    public InternalSnowflake GuildScheduledEventId { get; init; } = null!;

    /// <summary>
    /// The id of the user.
    /// </summary>
    [JsonPropertyName("user_id")]
    public InternalSnowflake UserId { get; init; } = null!;

    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public InternalSnowflake GuildId { get; init; } = null!;
}
