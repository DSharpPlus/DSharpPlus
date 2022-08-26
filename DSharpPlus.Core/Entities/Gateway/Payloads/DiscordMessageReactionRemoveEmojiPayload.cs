using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a bot removes all instances of a given emoji from the reactions of a message.
    /// </summary>
    [DiscordGatewayPayload("MESSAGE_REACTION_REMOVE_EMOJI")]
    public sealed record DiscordMessageReactionRemoveEmojiPayload
    {
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
        /// The emoji that was removed.
        /// </summary>
        [JsonPropertyName("emoji")]
        public DiscordEmoji Emoji { get; init; } = null!;
    }
}
