using System.Text.Json.Serialization;

/// <summary>
/// Represents an identify payload in the Discord voice protocol.
/// This payload is sent by the client to the voice server to
/// authenticate and establish a new voice connection.
/// </summary>
public class VoiceIdentifyPayload
{
    /// <summary>
    /// Gets or sets the operation code (<c>op</c>) that specifies
    /// the type of voice event or instruction.
    /// </summary>
    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    /// <summary>
    /// Gets or sets the identify data (<c>d</c>) containing
    /// authentication and connection details required to
    /// establish the voice session.
    /// </summary>
    [JsonPropertyName("d")]
    public VoiceIdentifyData Data { get; set; }
}
