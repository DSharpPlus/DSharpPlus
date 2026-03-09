using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol;

/// <summary>
/// Represents the inner payload of <see cref="VoiceStateUpdateEvent"/> 
/// </summary>
internal sealed record VoiceStateUpdateData
{
    [JsonPropertyName("channel_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public required ulong ChannelId { get; init; }

    [JsonPropertyName("guild_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public required ulong GuildId { get; init; }

    [JsonPropertyName("self_mute")]
    public required bool Mute { get; init; }

    [JsonPropertyName("self_deaf")]
    public required bool Deafen { get; init; }
}