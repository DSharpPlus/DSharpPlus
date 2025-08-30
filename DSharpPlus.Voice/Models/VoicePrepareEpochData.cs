using System.Text.Json.Serialization;

/// <summary>
/// Represents the data included in a voice "prepare epoch" payload.
/// This data provides information about the cryptographic epoch and
/// protocol version the voice server is preparing to transition to.
/// </summary>
public class VoicePrepareEpochData
{
    /// <summary>
    /// Gets or sets the target protocol version that the new
    /// epoch will use.
    /// </summary>
    [JsonPropertyName("protocol_version")]
    public ushort ProtocolVersion { get; set; }

    /// <summary>
    /// Gets or sets the epoch identifier. This is an incrementing
    /// number representing the cryptographic state version for
    /// the voice session.
    /// </summary>
    [JsonPropertyName("epoch")]
    public int Epoch { get; set; }
}
