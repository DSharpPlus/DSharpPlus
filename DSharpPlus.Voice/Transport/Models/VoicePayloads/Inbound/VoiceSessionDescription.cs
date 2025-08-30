using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the details of a voice session description,
/// typically received as part of the Discord voice WebSocket
/// session negotiation process.
/// </summary>
public class VoiceSessionDescription
{
    /// <summary>
    /// Gets or sets the name of the video codec in use
    /// for the session (e.g., "H264").
    /// </summary>
    [JsonPropertyName("video_codec")]
    public string VideoCodec { get; set; }

    /// <summary>
    /// Gets or sets the secure frames version used for
    /// encryption and decoding.
    /// </summary>
    [JsonPropertyName("secure_frames_version")]
    public int SecureFramesVersion { get; set; }

    /// <summary>
    /// Gets or sets the secret key used for SRTP (Secure Real-Time Protocol)
    /// encryption. Provided as an array of bytes.
    /// </summary>
    [JsonPropertyName("secret_key")]
    public List<byte> SecretKey { get; set; }

    /// <summary>
    /// Gets or sets the encryption mode used by the voice connection
    /// (e.g., "xsalsa20_poly1305").
    /// </summary>
    [JsonPropertyName("mode")]
    public string Mode { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the active media session.
    /// </summary>
    [JsonPropertyName("media_session_id")]
    public string MediaSessionId { get; set; }

    /// <summary>
    /// Gets or sets the version of the custom Dave protocol,
    /// if applicable to this session.
    /// </summary>
    [JsonPropertyName("dave_protocol_version")]
    public ushort DaveProtocolVersion { get; set; }

    /// <summary>
    /// Gets or sets the name of the audio codec in use
    /// for the session (e.g., "opus").
    /// </summary>
    [JsonPropertyName("audio_codec")]
    public string AudioCodec { get; set; }
}
