using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Payloads
{
    /// <summary>
    /// Sent when a user starts typing in a channel.
    /// </summary>
    [DiscordGatewayPayload("TYPING_START")]
    public sealed record DiscordTypingStartPayload
    {
        /// <summary>
        /// The id of the channel.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// The id of the user.
        /// </summary>
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake UserId { get; init; } = null!;

        /// <summary>
        /// The unix time (in seconds) of when the user started typing.
        /// </summary>
        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public int Timestamp { get; init; }

        /// <summary>
        /// The member who started typing if this happened in a guild.
        /// </summary>
        [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuildMember Member { get; init; } = null!;
    }
}
