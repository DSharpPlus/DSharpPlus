using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when a user is removed from a guild (leave/kick/ban).
/// </summary>
public sealed record InternalGuildMemberRemovePayload
{
    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public InternalSnowflake GuildId { get; init; } = null!;

    /// <summary>
    /// The user who was removed.
    /// </summary>
    [JsonPropertyName("user")]
    public InternalUser User { get; init; } = null!;
}
