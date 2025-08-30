using System.Text.Json.Serialization;

/// <summary>
/// Represents a payload for the "prepare epoch" event in the
/// Discord voice protocol. This payload is sent when the voice
/// server begins preparing a new cryptographic epoch or version.
/// </summary>
public class VoicePrepareEpochPayload
{
    /// <summary>
    /// Gets or sets the sequence number (<c>seq</c>) associated with
    /// this payload. Used to maintain message ordering.
    /// </summary>
    [JsonPropertyName("seq")]
    public int Sequence { get; set; }

    /// <summary>
    /// Gets or sets the operation code (<c>op</c>) that specifies
    /// the type of voice event or instruction.
    /// </summary>
    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    /// <summary>
    /// Gets or sets the data object (<c>d</c>) containing details
    /// about the epoch preparation, such as protocol version and epoch ID.
    /// </summary>
    [JsonPropertyName("d")]
    public VoicePrepareEpochData Data { get; set; }
}
