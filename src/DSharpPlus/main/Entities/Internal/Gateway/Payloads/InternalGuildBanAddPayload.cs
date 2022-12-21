using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when a user is banned from a guild.
/// </summary>
public sealed record InternalGuildBanAddPayload
{
    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; init; } = null!;

    /// <summary>
    /// The banned user.
    /// </summary>
    [JsonPropertyName("user")]
    public InternalUser User { get; init; } = null!;
}
