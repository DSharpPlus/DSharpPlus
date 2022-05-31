using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Payloads
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
        [JsonProperty("guild_scheduled_event_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildScheduledEventId { get; init; } = null!;

        /// <summary>
        /// The id of the user.
        /// </summary>
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake UserId { get; init; } = null!;

        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;
    }
}
