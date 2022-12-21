using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a user has unsubscribed from a guild scheduled event.
    /// </summary>
    [DiscordGatewayPayload("GUILD_SCHEDULED_EVENT_USER_REMOVE")]
    public sealed record DiscordGuildScheduledEventUserRemovePayload
    {
        /// <summary>
        /// The id of the guild scheduled event.
        /// </summary>
        [JsonPropertyName("guild_scheduled_event_id")]
        public DiscordSnowflake GuildScheduledEventId { get; init; } = null!;

        /// <summary>
        /// The id of the user.
        /// </summary>
        [JsonPropertyName("user_id")]
        public DiscordSnowflake UserId { get; init; } = null!;

        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;
    }
}
