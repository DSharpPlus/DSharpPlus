using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when a guild role is deleted.
/// </summary>
public sealed record InternalGuildRoleDeletePayload
{
    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; init; } = null!;

    /// <summary>
    /// The role deleted.
    /// </summary>
    [JsonPropertyName("role_id")]
    public Snowflake RoleId { get; init; } = null!;
}
