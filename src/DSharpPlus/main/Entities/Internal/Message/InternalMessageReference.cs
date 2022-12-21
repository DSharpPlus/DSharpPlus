using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <summary>
/// See https://discord.com/developers/docs/resources/channel#message-reference-object-message-reference-structure
/// </summary>
public sealed record InternalMessageReference
{
    /// <summary>
    /// The id of the originating message.
    /// </summary>
    [JsonPropertyName("message_id")]
    public Optional<Snowflake> MessageId { get; init; }

    /// <summary>
    /// The id of the originating message's channel.
    /// </summary>
    /// <remarks>
    /// channel_id is optional when creating a reply, but will always be present when receiving an event/response that includes this data model.
    /// </remarks>
    [JsonPropertyName("channel_id")]
    public Optional<Snowflake> ChannelId { get; init; }

    /// <summary>
    /// The id of the originating message's guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Optional<Snowflake> GuildId { get; init; }

    /// <summary>
    /// When sending, whether to error if the referenced message doesn't exist instead of sending as a normal (non-reply) message, default true.
    /// </summary>
    [JsonPropertyName("fail_if_not_exists")]
    public Optional<bool> FailIfNotExists { get; init; }
}
