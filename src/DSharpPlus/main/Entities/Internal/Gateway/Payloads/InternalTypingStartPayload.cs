using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a user starts typing in a channel.
    /// </summary>
    [InternalGatewayPayload("TYPING_START")]
    public sealed record InternalTypingStartPayload
    {
        /// <summary>
        /// The id of the channel.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public InternalSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public Optional<InternalSnowflake> GuildId { get; init; }

        /// <summary>
        /// The id of the user.
        /// </summary>
        [JsonPropertyName("user_id")]
        public InternalSnowflake UserId { get; init; } = null!;

        /// <summary>
        /// The unix time (in seconds) of when the user started typing.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public int Timestamp { get; init; }

        /// <summary>
        /// The member who started typing if this happened in a guild.
        /// </summary>
        [JsonPropertyName("member")]
        public InternalGuildMember Member { get; init; } = null!;
    }
}
