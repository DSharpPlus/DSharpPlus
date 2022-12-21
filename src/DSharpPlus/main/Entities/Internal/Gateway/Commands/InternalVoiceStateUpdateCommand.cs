using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Commands;

public sealed record InternalVoiceStateUpdateCommand
{
    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; init; } = null!;

    /// <summary>
    /// The id of the voice channel the client wants to join (null if disconnecting).
    /// </summary>
    [JsonPropertyName("channel_id")]
    public Snowflake? ChannelId { get; init; }

    /// <summary>
    /// Is the client muted.
    /// </summary>
    [JsonPropertyName("self_mute")]
    public bool SelfMute { get; init; }

    /// <summary>
    /// Is the client deafened.
    /// </summary>
    [JsonPropertyName("self_deaf")]
    public bool SelfDeaf { get; init; }
}
