// See https://aka.ms/new-console-template for more information
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class VoiceSessionDescription
{
    [JsonPropertyName("video_codec")]
    public string VideoCodec { get; set; }

    [JsonPropertyName("secure_frames_version")]
    public int SecureFramesVersion { get; set; }

    // JSON provides an array of numbers; List<byte> maps cleanly without a custom converter.
    [JsonPropertyName("secret_key")]
    public List<byte> SecretKey { get; set; }

    [JsonPropertyName("mode")]
    public string Mode { get; set; }

    [JsonPropertyName("media_session_id")]
    public string MediaSessionId { get; set; }

    [JsonPropertyName("dave_protocol_version")]
    public int DaveProtocolVersion { get; set; }

    [JsonPropertyName("audio_codec")]
    public string AudioCodec { get; set; }
}
