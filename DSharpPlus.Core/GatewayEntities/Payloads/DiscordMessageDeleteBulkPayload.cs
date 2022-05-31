using System;
using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Payloads
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
        [JsonProperty("ids", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordSnowflake> Ids { get; init; } = Array.Empty<DiscordSnowflake>();

        /// <summary>
        /// The id of the channel.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> GuildId { get; init; }
    }
}
