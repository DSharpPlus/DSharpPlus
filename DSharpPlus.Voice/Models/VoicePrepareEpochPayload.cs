// See https://aka.ms/new-console-template for more information
using System.Text.Json.Serialization;

public class VoicePrepareEpochPayload
{
    [JsonPropertyName("seq")]
    public int Sequence { get; set; }

    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    [JsonPropertyName("d")]
    public VoicePrepareEpochData Data { get; set; }
}
