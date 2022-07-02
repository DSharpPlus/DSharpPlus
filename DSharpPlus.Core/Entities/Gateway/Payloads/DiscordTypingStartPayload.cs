using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
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
        [JsonPropertyName("channel_id")]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// The id of the user.
        /// </summary>
        [JsonPropertyName("user_id")]
        public DiscordSnowflake UserId { get; init; } = null!;

        /// <summary>
        /// The unix time (in seconds) of when the user started typing.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public int Timestamp { get; init; }

        /// <summary>
        /// The member who started typing if this happened in a guild.
        /// </summary>
        [JsonPropertyName("member")]
        public DiscordGuildMember Member { get; init; } = null!;
    }
}
