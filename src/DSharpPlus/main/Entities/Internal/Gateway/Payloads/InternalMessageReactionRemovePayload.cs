using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when a user removes a reaction from a message.
/// </summary>
public sealed record InternalMessageReactionRemovePayload
{
    /// <summary>
    /// The id of the user.
    /// </summary>
    [JsonPropertyName("user_id")]
    public InternalSnowflake UserId { get; init; } = null!;

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

    /// <summary>
    /// The emoji used to react.
    /// </summary>
    [JsonPropertyName("emoji")]
    public InternalEmoji Emoji { get; init; } = null!;
}
