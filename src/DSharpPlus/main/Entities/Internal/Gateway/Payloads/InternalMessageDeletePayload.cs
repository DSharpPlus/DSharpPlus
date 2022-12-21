using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when a message is deleted.
/// </summary>
public sealed record InternalMessageDeletePayload
{
    /// <summary>
    /// The id of the message.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; init; } = null!;

    /// <summary>
    /// The id of the channel.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public Snowflake ChannelId { get; init; } = null!;

    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Optional<Snowflake> GuildId { get; init; }
}
