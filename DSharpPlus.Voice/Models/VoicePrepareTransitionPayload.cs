// See https://aka.ms/new-console-template for more information
using System.Text.Json.Serialization;

public class VoicePrepareTransitionPayload
{
    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    [JsonPropertyName("d")]
    public VoicePrepareTransitionData Data { get; set; }
}
