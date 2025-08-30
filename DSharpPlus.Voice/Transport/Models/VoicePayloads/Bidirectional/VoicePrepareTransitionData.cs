using System.Text.Json.Serialization;

/// <summary>
/// Represents the data included in a voice "prepare transition" payload.
/// This data indicates details about a pending protocol or version transition
/// for the active Discord voice connection.
/// </summary>
public class VoicePrepareTransitionData
{
    /// <summary>
    /// Gets or sets the target protocol version that the voice
    /// connection is preparing to transition to.
    /// </summary>
    [JsonPropertyName("protocol_version")]
    public int ProtocolVersion { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the transition process.
    /// This ID can be used to track and correlate the protocol upgrade
    /// or downgrade request.
    /// </summary>
    [JsonPropertyName("transition_id")]
    public uint TransitionId { get; set; }
}
