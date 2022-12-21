using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when a guild role is created.
/// </summary>
public sealed record InternalGuildRoleCreatePayload
{
    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; init; } = null!;

    /// <summary>
    /// The role created.
    /// </summary>
    [JsonPropertyName("role")]
    public InternalRole Role { get; init; } = null!;
}
