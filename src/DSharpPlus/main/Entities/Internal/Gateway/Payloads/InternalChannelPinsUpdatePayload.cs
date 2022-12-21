using System;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when a message is pinned or unpinned in a text channel. This is not sent when a pinned message is deleted.
    /// </summary>
    [InternalGatewayPayload("CHANNEL_PINS_UPDATE")]
    public sealed record InternalChannelPinsUpdatePayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public Optional<InternalSnowflake> GuildId { get; init; }

        /// <summary>
        /// The id of the channel.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public InternalSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The time at which the most recent pinned message was pinned.
        /// </summary>
        [JsonPropertyName("last_pin_timestamp")]
        public Optional<DateTimeOffset?> LastPinTimestamp { get; init; }
    }
}
