using System;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when a message is pinned or unpinned in a text channel. This is not sent when a pinned message is deleted.
/// </summary>
public sealed record InternalChannelPinsUpdatePayload
{
    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Optional<Snowflake> GuildId { get; init; }

    /// <summary>
    /// The id of the channel.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public Snowflake ChannelId { get; init; } = null!;

    /// <summary>
    /// The time at which the most recent pinned message was pinned.
    /// </summary>
    [JsonPropertyName("last_pin_timestamp")]
    public Optional<DateTimeOffset?> LastPinTimestamp { get; init; }
}
