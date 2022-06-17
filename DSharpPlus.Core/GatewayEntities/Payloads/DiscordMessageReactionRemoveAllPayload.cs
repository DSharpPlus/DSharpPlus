using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.GatewayEntities.Payloads
{
    /// <summary>
    /// Sent when a user explicitly removes all reactions from a message.
    /// </summary>
    [DiscordGatewayPayload("MESSAGE_REACTION_REMOVE_ALL")]
    public sealed record DiscordMessageReactionRemoveAllPayload
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
    }
}
