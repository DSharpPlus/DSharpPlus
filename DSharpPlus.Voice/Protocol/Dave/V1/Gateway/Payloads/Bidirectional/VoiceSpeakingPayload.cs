using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Bidirectional;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.Speaking"/>.
/// </summary>
internal sealed record VoiceSpeakingPayload : IVoicePayload
{
    /// <summary>
    /// Controls how the application is allowed to speak.
    /// </summary>
    [JsonPropertyName("speaking")]
    public VoiceSpeakingFlags SpeakingMode { get; init; }

    /// <summary>
    /// This should always be set to 0.
    /// </summary>
    [JsonPropertyName("delay")]
    public int Delay { get; init; }

    /// <summary>
    /// Updates the client's SSRC.
    /// </summary>
    [JsonPropertyName("ssrc")]
    public int SSRC { get; init; }
}
