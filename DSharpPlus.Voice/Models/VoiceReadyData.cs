using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the data included in a <c>VOICE_READY</c> payload,
/// sent by the Discord voice server after a successful connection.
/// Provides the SSRC, connection details, supported modes, and
/// heartbeat interval required for maintaining the voice session.
/// </summary>
public class VoiceReadyData
{
    /// <summary>
    /// Gets or sets the synchronization source (SSRC) identifier
    /// assigned to the client for RTP packets.
    /// </summary>
    [JsonPropertyName("ssrc")]
    public uint Ssrc { get; set; }

    /// <summary>
    /// Gets or sets the external IP address of the client as
    /// observed by the voice server.
    /// </summary>
    [JsonPropertyName("ip")]
    public string Ip { get; set; }

    /// <summary>
    /// Gets or sets the UDP port assigned by the voice server
    /// for RTP communication.
    /// </summary>
    [JsonPropertyName("port")]
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the list of supported encryption modes
    /// (e.g., <c>xsalsa20_poly1305</c>) that can be used for
    /// securing voice data.
    /// </summary>
    [JsonPropertyName("modes")]
    public List<string> Modes { get; set; }

    /// <summary>
    /// Gets or sets the heartbeat interval (in milliseconds)
    /// that the client should use to send keepalive packets
    /// to the voice server.
    /// </summary>
    [JsonPropertyName("heartbeat_interval")]
    public int HeartbeatInterval { get; set; }
}
