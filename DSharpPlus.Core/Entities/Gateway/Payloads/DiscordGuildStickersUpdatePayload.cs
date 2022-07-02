using System;
using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
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
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// An array of stickers.
        /// </summary>
        [JsonPropertyName("stickers")]
        public IReadOnlyList<DiscordSticker> Stickers { get; init; } = Array.Empty<DiscordSticker>();
    }
}
