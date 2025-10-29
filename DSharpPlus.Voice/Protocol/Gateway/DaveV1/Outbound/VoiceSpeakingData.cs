using System.Text.Json.Serialization;

using DSharpPlus.Voice.Protocol.Gateway.DaveV1.Bidirectional;

namespace DSharpPlus.Voice.Protocol.Gateway.DaveV1.Outbound;
/// <summary>
/// Represents the data included in a voice "speaking" event.
/// This event indicates that a user has started or stopped
/// transmitting audio in a Discord voice channel.
/// </summary>
public class VoiceSpeakingData
{
    /// <summary>
    /// Gets or sets the speaking flags that indicate the user's
    /// speaking state. This is a bitfield representing whether
    /// the user is speaking via microphone, soundshare, etc.
    /// </summary>
    [JsonPropertyName("speaking")]
    public VoiceSpeakingFlags Speaking { get; set; }

    /// <summary>
    /// Gets or sets the delay (in milliseconds) before audio data
    /// begins being transmitted after speaking is flagged.
    /// </summary>
    [JsonPropertyName("delay")]
    public int Delay { get; set; }

    /// <summary>
    /// Gets or sets the SSRC (synchronization source) identifier
    /// for the RTP stream associated with the user.
    /// </summary>
    [JsonPropertyName("ssrc")]
    public uint Ssrc { get; set; }
}
