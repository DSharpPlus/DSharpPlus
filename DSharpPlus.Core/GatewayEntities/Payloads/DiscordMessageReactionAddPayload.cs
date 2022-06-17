using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.GatewayEntities.Payloads
{
    /// <summary>
    /// Sent when a user adds a reaction to a message.
    /// </summary>
    [DiscordGatewayPayload("MESSAGE_REACTION_ADD")]
    public sealed record DiscordMessageReactionAddPayload
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
        /// The member who reacted if this happened in a guild.
        /// </summary>
        [JsonPropertyName("member")]
        public Optional<DiscordGuildMember> Member { get; init; }

        /// <summary>
        /// The emoji used to react.
        /// </summary>
        [JsonPropertyName("emoji")]
        public DiscordEmoji Emoji { get; init; } = null!;
    }
}
