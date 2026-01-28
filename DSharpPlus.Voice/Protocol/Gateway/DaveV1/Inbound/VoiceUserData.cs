
using System.Text.Json.Serialization;

public class VoiceUserData
{
    /// <summary>
    /// Gets/Sets the user id
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
}
