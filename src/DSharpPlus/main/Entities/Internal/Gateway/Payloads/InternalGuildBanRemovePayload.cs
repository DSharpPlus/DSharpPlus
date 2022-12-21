using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when a user is unbanned from a guild.
/// </summary>
public sealed record InternalGuildBanRemovePayload
{
    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public InternalSnowflake GuildId { get; init; } = null!;

    /// <summary>
    /// The unbanned user.
    /// </summary>
    [JsonPropertyName("user")]
    public InternalUser User { get; init; } = null!;
}
