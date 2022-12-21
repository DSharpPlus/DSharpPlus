using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when an invite is deleted.
/// </summary>
public sealed record InternalInviteDeletePayload
{
    /// <summary>
    /// The channel of the invite.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public Snowflake ChannelId { get; init; } = null!;

    /// <summary>
    /// The guild of the invite.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Optional<Snowflake> GuildId { get; init; }

    /// <summary>
    /// The unique invite code.
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; init; } = null!;
}
