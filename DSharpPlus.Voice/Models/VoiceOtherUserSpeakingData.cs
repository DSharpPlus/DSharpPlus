using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Models;
/// <summary>
/// Represents the data for another user's speaking state in a voice channel.
/// This event is sent by the Discord voice server to notify clients when
/// another user begins or stops transmitting audio.
/// </summary>
public class VoiceOtherUserSpeakingData
{
    /// <summary>
    /// Gets or sets the speaking flags indicating the user's
    /// current speaking state. This is a bitfield that shows whether
    /// the user is speaking via microphone, soundshare, etc.
    /// </summary>
    [JsonPropertyName("speaking")]
    public int Speaking { get; set; }

    /// <summary>
    /// Gets or sets the SSRC (synchronization source) identifier
    /// for the RTP stream associated with the user.
    /// </summary>
    [JsonPropertyName("ssrc")]
    public int Ssrc { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user associated with this
    /// speaking event.
    /// </summary>
    [JsonPropertyName("user_id")]
    public ulong UserId { get; set; }
}
