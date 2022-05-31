using System;
using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Payloads
{
    /// <summary>
    /// Sent when a guild's emojis have been updated.
    /// </summary>
    [DiscordGatewayPayload("GUILD_STICKERS_UPDATE")]
    public sealed record DiscordGuildStickersUpdatePayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// An array of stickers.
        /// </summary>
        [JsonProperty("stickers", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordSticker> Stickers { get; init; } = Array.Empty<DiscordSticker>();
    }
}
