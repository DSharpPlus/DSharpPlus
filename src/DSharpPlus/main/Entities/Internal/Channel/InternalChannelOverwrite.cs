using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <remarks>
/// See <see href="https://discord.com/developers/docs/topics/permissions#permissions">permissions</see> for more information about the allow and deny fields.
/// </remarks>
public sealed record InternalChannelOverwrite
{
    /// <summary>
    /// Role or user id.
    /// </summary>
    [JsonPropertyName("id")]
    public InternalSnowflake Id { get; init; } = null!;

    /// <summary>
    /// Either 0 (role) or 1 (member).
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; init; }

    /// <summary>
    /// Permission bit set.
    /// </summary>
    [JsonPropertyName("allow")]
    public DiscordPermissions Allow { get; init; }

    /// <summary>
    /// Permission bit set.
    /// </summary>
    [JsonPropertyName("deny")]
    public DiscordPermissions Deny { get; init; }

    public static implicit operator ulong(InternalChannelOverwrite channelOverwrite) => channelOverwrite.Id;
    public static implicit operator InternalSnowflake(InternalChannelOverwrite channelOverwrite) => channelOverwrite.Id;
}
