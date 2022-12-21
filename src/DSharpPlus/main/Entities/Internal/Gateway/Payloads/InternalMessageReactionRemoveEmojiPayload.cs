using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a bot removes all instances of a given emoji from the reactions of a message.
    /// </summary>
    [InternalGatewayPayload("MESSAGE_REACTION_REMOVE_EMOJI")]
    public sealed record InternalMessageReactionRemoveEmojiPayload
    {
        /// <summary>
        /// The id of the channel.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public InternalSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The id of the message.
        /// </summary>
        [JsonPropertyName("message_id")]
        public InternalSnowflake MessageId { get; init; } = null!;

        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public Optional<InternalSnowflake> GuildId { get; init; }

        /// <summary>
        /// The emoji that was removed.
        /// </summary>
        [JsonPropertyName("emoji")]
        public InternalEmoji Emoji { get; init; } = null!;
    }
}
