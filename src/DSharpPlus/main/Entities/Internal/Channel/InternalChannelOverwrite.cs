using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <remarks>
/// See <see href="https://discord.com/developers/docs/topics/permissions#permissions">permissions</see> 
/// for more information about the allow and deny fields.
/// </remarks>
public sealed record InternalChannelOverwrite
{
    /// <summary>
    /// Role or user id.
    /// </summary>
    [JsonPropertyName("id")]
    public required Snowflake Id { get; init; }

    /// <summary>
    /// Either 0 (role) or 1 (member).
    /// </summary>
    [JsonPropertyName("type")]
    public required int Type { get; init; }

    /// <summary>
    /// Permission bit set.
    /// </summary>
    [JsonPropertyName("allow")]
    public required DiscordPermissions Allow { get; init; }

    /// <summary>
    /// Permission bit set.
    /// </summary>
    [JsonPropertyName("deny")]
    public required DiscordPermissions Deny { get; init; }

    public static implicit operator ulong(InternalChannelOverwrite channelOverwrite) => channelOverwrite.Id;
    public static implicit operator Snowflake(InternalChannelOverwrite channelOverwrite) => channelOverwrite.Id;
}
