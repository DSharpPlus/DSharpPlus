using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a user removes a reaction from a message.
    /// </summary>
    [DiscordGatewayPayload("MESSAGE_REACTION_REMOVE")]
    public sealed record DiscordMessageReactionRemovePayload
    {
        /// <summary>
        /// The id of the user.
        /// </summary>
        [JsonPropertyName("user_id")]
        public DiscordSnowflake UserId { get; init; } = null!;

        /// <summary>
        /// The id of the channel.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The id of the message.
        /// </summary>
        [JsonPropertyName("message_id")]
        public DiscordSnowflake MessageId { get; init; } = null!;

        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// The emoji used to react.
        /// </summary>
        [JsonPropertyName("emoji")]
        public DiscordEmoji Emoji { get; init; } = null!;
    }
}
