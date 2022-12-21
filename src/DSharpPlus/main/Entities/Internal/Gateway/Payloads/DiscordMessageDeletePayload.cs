using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a message is deleted.
    /// </summary>
    [DiscordGatewayPayload("MESSAGE_DELETE")]
    public sealed record DiscordMessageDeletePayload
    {
        /// <summary>
        /// The id of the message.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

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
    }
}
