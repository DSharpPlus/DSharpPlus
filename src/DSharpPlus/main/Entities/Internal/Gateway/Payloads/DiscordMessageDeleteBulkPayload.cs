using System;
using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when multiple messages are deleted at once.
    /// </summary>
    [DiscordGatewayPayload("MESSAGE_DELETE_BULK")]
    public sealed record DiscordMessageDeleteBulkPayload
    {
        /// <summary>
        /// The id of the messages.
        /// </summary>
        [JsonPropertyName("ids")]
        public IReadOnlyList<DiscordSnowflake> Ids { get; init; } = Array.Empty<DiscordSnowflake>();

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
