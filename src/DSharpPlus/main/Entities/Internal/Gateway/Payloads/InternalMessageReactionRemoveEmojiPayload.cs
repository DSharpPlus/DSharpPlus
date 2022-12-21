using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when a bot removes all instances of a given emoji from the reactions of a message.
/// </summary>
public sealed record InternalMessageReactionRemoveEmojiPayload
{
    /// <summary>
    /// The id of the channel.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public Snowflake ChannelId { get; init; } = null!;

    /// <summary>
    /// The id of the message.
    /// </summary>
    [JsonPropertyName("message_id")]
    public Snowflake MessageId { get; init; } = null!;

    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Optional<Snowflake> GuildId { get; init; }

    /// <summary>
    /// The emoji that was removed.
    /// </summary>
    [JsonPropertyName("emoji")]
    public InternalEmoji Emoji { get; init; } = null!;
}
