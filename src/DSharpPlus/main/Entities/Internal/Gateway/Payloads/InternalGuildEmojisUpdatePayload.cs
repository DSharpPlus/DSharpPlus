using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when a guild's emojis have been updated.
/// </summary>
public sealed record InternalGuildEmojisUpdatePayload
{
    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public InternalSnowflake GuildId { get; init; } = null!;

    /// <summary>
    /// An array of emojis.
    /// </summary>
    [JsonPropertyName("emojis")]
    public IReadOnlyList<InternalEmoji> Emojis { get; init; } = Array.Empty<InternalEmoji>();
}
