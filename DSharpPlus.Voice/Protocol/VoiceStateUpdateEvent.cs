using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol;

/// <summary>
/// Represents the payload we send to the main gateway to tell Discord we want to join voice.
/// </summary>
internal sealed record VoiceStateUpdateEvent
{
    [JsonPropertyName("op")]
    public int Opcode => 4;

    [JsonPropertyName("d")]
    public required VoiceStateUpdateData Data { get; init; }
}