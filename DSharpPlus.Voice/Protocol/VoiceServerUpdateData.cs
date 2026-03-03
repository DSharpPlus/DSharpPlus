using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol;

/// <summary>
/// Represents the inner payload of <see cref="VoiceServerUpdateEvent"/> 
/// </summary>
internal sealed record VoiceServerUpdateData
{
    [JsonPropertyName("channel_id")]
    public required ulong ChannelId { get; init; }

    [JsonPropertyName("guild_id")]
    public required ulong GuildId { get; init; }

    [JsonPropertyName("self_mute")]
    public required bool Mute { get; init; }

    [JsonPropertyName("self_deaf")]
    public required bool Deafen { get; init; }
}