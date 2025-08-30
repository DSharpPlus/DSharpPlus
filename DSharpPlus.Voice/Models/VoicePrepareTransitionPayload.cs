using System.Text.Json.Serialization;

/// <summary>
/// Represents a payload for the "prepare transition" event in the
/// Discord voice protocol. This payload indicates that the voice
/// server is preparing to transition protocols or versions.
/// </summary>
public class VoicePrepareTransitionPayload
{
    /// <summary>
    /// Gets or sets the operation code (<c>op</c>) that specifies
    /// the type of voice event or instruction.
    /// </summary>
    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    /// <summary>
    /// Gets or sets the data object (<c>d</c>) containing details
    /// about the protocol transition preparation.
    /// </summary>
    [JsonPropertyName("d")]
    public VoicePrepareTransitionData Data { get; set; }
}
