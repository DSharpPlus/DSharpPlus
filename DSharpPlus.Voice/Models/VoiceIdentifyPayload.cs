// See https://aka.ms/new-console-template for more information
using System.Text.Json.Serialization;

public class VoiceIdentifyPayload
{
    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    [JsonPropertyName("d")]
    public VoiceIdentifyData Data { get; set; }
}
