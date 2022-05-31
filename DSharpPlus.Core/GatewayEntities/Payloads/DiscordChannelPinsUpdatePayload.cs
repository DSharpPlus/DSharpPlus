using System;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Payloads
{
    /// <summary>
    /// Sent when a message is pinned or unpinned in a text channel. This is not sent when a pinned message is deleted.
    /// </summary>
    [DiscordGatewayPayload("CHANNEL_PINS_UPDATE")]
    public sealed record DiscordChannelPinsUpdatePayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// The id of the channel.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The time at which the most recent pinned message was pinned.
        /// </summary>
        [JsonProperty("last_pin_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DateTimeOffset?> LastPinTimestamp { get; init; }
    }
}
