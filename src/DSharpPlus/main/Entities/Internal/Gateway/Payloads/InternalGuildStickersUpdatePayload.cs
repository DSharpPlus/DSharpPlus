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
    [InternalGatewayPayload("GUILD_STICKERS_UPDATE")]
    public sealed record InternalGuildStickersUpdatePayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public InternalSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// An array of stickers.
        /// </summary>
        [JsonPropertyName("stickers")]
        public IReadOnlyList<InternalSticker> Stickers { get; init; } = Array.Empty<InternalSticker>();
    }
}
