// See https://aka.ms/new-console-template for more information
using System.Text.Json.Serialization;

public class VoicePrepareTransitionData
{
    [JsonPropertyName("protocol_version")]
    public int ProtocolVersion { get; set; }

    [JsonPropertyName("transition_id")]
    public uint TransitionId { get; set; }
}