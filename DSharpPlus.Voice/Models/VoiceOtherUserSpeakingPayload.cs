using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Models;
/// <summary>
/// Represents a payload for when another user in the voice channel
/// begins or stops speaking. This event is raised by the Discord
/// voice server to inform clients about changes in other users’
/// speaking states.
/// </summary>
public class VoiceOtherUserSpeakingPayload
{
    /// <summary>
    /// Gets or sets the operation code (<c>op</c>) that specifies
    /// the type of voice event or instruction.
    /// </summary>
    [JsonPropertyName("op")]
    public int OpCode { get; set; }

    /// <summary>
    /// Gets or sets the data object (<c>d</c>) containing information
    /// about the other user’s speaking state.
    /// </summary>
    [JsonPropertyName("d")]
    public VoiceOtherUserSpeakingData Data { get; set; }
}
