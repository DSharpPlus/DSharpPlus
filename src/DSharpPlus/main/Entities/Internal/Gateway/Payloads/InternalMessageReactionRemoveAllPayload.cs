using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when a user explicitly removes all reactions from a message.
/// </summary>
public sealed record InternalMessageReactionRemoveAllPayload
{
    /// <summary>
    /// The id of the channel.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public InternalSnowflake ChannelId { get; init; } = null!;

    /// <summary>
    /// The id of the message.
    /// </summary>
    [JsonPropertyName("message_id")]
    public InternalSnowflake MessageId { get; init; } = null!;

    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Optional<InternalSnowflake> GuildId { get; init; }
}
