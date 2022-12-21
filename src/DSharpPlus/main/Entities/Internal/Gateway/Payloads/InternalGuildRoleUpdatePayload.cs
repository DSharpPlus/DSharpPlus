using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when a guild role is updated.
/// </summary>
public sealed record InternalGuildRoleUpdatePayload
{
    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; init; } = null!;

    /// <summary>
    /// The role updated.
    /// </summary>
    [JsonPropertyName("role")]
    public InternalRole Role { get; init; } = null!;
}
