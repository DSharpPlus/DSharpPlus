using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol;

internal sealed record RequestChannelInfoEvent
{
    [JsonPropertyName("op")]
    public int Opcode => 43;

    [JsonPropertyName("d")]
    public required RequestChannelInfoData Data { get; init; }
}
