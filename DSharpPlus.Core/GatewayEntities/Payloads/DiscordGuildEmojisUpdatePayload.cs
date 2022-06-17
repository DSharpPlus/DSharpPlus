using System;
using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.GatewayEntities.Payloads
{
    /// <summary>
    /// Sent when a guild's emojis have been updated.
    /// </summary>
    [DiscordGatewayPayload("GUILD_EMOJIS_UPDATE")]
    public sealed record DiscordGuildEmojisUpdatePayload
    {
        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// An array of emojis.
        /// </summary>
        [JsonPropertyName("emojis")]
        public IReadOnlyList<DiscordEmoji> Emojis { get; init; } = Array.Empty<DiscordEmoji>();
    }
}
