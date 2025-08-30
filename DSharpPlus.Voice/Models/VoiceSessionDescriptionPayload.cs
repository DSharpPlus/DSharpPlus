using System.Text.Json.Serialization;

/// <summary>
/// Represents a voice session description payload, which is used
/// in the Discord voice WebSocket protocol to deliver session details.
/// </summary>
public class VoiceSessionDescriptionPayload
{
    /// <summary>
    /// Gets or sets the operation code (<c>op</c>) that indicates
    /// the type of voice event or instruction.
    /// </summary>
    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    /// <summary>
    /// Gets or sets the session description data (<c>d</c>) containing
    /// encryption and connection details for the voice session.
    /// </summary>
    [JsonPropertyName("d")]
    public VoiceSessionDescription Data { get; set; }
}
