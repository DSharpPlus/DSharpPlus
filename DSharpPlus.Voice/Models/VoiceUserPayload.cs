using System.Text.Json.Serialization;

public class VoiceUserPayload
{
    /// <summary>
    /// Gets/Sets the sequence number
    /// </summary>
    [JsonPropertyName("seq")]
    public int Sequence { get; set; }
    /// <summary>
    /// Gets/Sets the op code
    /// </summary>

    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    /// <summary>
    /// Gets/Sets the payload
    /// </summary>
    [JsonPropertyName("d")]
    public VoiceUserData Data { get; set; }
}
