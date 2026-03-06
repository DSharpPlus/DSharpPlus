using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Gateway.Payloads.Bidirectional;

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
    [JsonPropertyName("delay"), JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]
    public int Delay { get; init; }

    /// <summary>
    /// Updates the client's SSRC.
    /// </summary>
    [JsonPropertyName("ssrc")]
    public uint SSRC { get; init; }

    /// <summary>
    /// The ID of the user who started speaking.
    /// </summary>
    [JsonPropertyName("user_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
    public ulong UserId { get; init; }
}
