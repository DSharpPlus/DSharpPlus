// See https://aka.ms/new-console-template for more information
using System.Text.Json.Serialization;

public class VoiceStateUpdatePayload
{
    [JsonPropertyName("t")]
    public string Type { get; set; }

    [JsonPropertyName("s")]
    public int Sequence { get; set; }

    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    [JsonPropertyName("d")]
    public VoiceStateUpdateData Data { get; set; }
}
