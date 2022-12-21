using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when a guild's emojis have been updated.
/// </summary>
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
