using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Models;

/// <summary>
/// Represents a voice speaking payload in the Discord voice protocol.
/// This payload is sent by the voice server to indicate that a user
/// has started or stopped transmitting audio in a voice channel.
/// </summary>
public class VoiceSpeakingPayload
{
    /// <summary>
    /// Gets or sets the operation code (<c>op</c>) that specifies
    /// the type of voice event or instruction.
    /// </summary>
    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    /// <summary>
    /// Gets or sets the data object (<c>d</c>) containing the user's
    /// speaking state, delay, and SSRC information.
    /// </summary>
    [JsonPropertyName("d")]
    public VoiceSpeakingData Data { get; set; }
}
