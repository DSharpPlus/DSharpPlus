using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when multiple messages are deleted at once.
/// </summary>
public sealed record InternalMessageDeleteBulkPayload
{
    /// <summary>
    /// The id of the messages.
    /// </summary>
    [JsonPropertyName("ids")]
    public IReadOnlyList<InternalSnowflake> Ids { get; init; } = Array.Empty<InternalSnowflake>();

    /// <summary>
    /// The id of the channel.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public InternalSnowflake ChannelId { get; init; } = null!;

    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Optional<InternalSnowflake> GuildId { get; init; }
}
