using System.Text.Json.Serialization;

/// <summary>
/// Represents a voice ready payload sent by the Discord voice server.
/// This payload is received after a successful voice connection is established
/// and contains session details required for communication.
/// </summary>
public class VoiceReadyPayload
{
    /// <summary>
    /// Gets or sets the operation code (<c>op</c>) that specifies
    /// the type of voice event or instruction.
    /// </summary>
    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    /// <summary>
    /// Gets or sets the data object (<c>d</c>) containing voice connection
    /// readiness details, such as SSRC, IP, port, and supported modes.
    /// </summary>
    [JsonPropertyName("d")]
    public VoiceReadyData Data { get; set; }
}
