using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a user has subscribed to a guild scheduled event.
    /// </summary>
    [InternalGatewayPayload("GUILD_SCHEDULED_EVENT_USER_ADD")]
    public sealed record InternalGuildScheduledEventUserAddPayload
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
}
