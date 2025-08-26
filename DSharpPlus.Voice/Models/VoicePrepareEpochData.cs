// See https://aka.ms/new-console-template for more information
using System.Text.Json.Serialization;

public class VoicePrepareEpochData
{
    [JsonPropertyName("protocol_version")]
    public ushort ProtocolVersion { get; set; }

    [JsonPropertyName("epoch")]
    public int Epoch { get; set; }
}
