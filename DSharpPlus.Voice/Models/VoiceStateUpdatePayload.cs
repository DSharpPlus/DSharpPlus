
using System.Text.Json.Serialization;

public class VoiceStateUpdatePayload
{
    /// <summary>
    /// Gets/Sets the type
    /// </summary>
    [JsonPropertyName("t")]
    public string Type { get; set; }
    /// <summary>
    /// Gets/Sets the sequence number
    /// </summary>
    [JsonPropertyName("s")]
    public int Sequence { get; set; }
    /// <summary>
    /// Gets/Sets the op code
    /// </summary>
    [JsonPropertyName("op")]
    public int OpCode { get; set; }
    /// <summary>
    /// Gets/Sets the data payload
    /// </summary>
    [JsonPropertyName("d")]
    public VoiceStateUpdateData Data { get; set; }
}
