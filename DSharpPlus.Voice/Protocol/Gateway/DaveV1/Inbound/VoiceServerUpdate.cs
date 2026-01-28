using System.Text.Json.Serialization;

/// <summary>
/// Represents a voice server update payload received from the
/// Discord Gateway. This event provides details about the voice
/// server connection and is required to establish a voice session.
/// </summary>
public class VoiceServerUpdate
{
    /// <summary>
    /// Gets or sets the event type (<c>t</c>), typically identifying
    /// this payload as a <c>VOICE_SERVER_UPDATE</c>.
    /// </summary>
    [JsonPropertyName("t")]
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the sequence number (<c>s</c>) for this event,
    /// used to maintain event ordering from the Gateway.
    /// </summary>
    [JsonPropertyName("s")]
    public int Sequence { get; set; }

    /// <summary>
    /// Gets or sets the operation code (<c>op</c>) that specifies the
    /// type of Gateway event.
    /// </summary>
    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    /// <summary>
    /// Gets or sets the data object (<c>d</c>) containing information
    /// about the voice server and authentication details.
    /// </summary>
    [JsonPropertyName("d")]
    public VoiceServerData Data { get; set; }
}
