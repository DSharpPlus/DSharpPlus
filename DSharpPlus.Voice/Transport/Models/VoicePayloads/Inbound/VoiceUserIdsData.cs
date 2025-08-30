
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class VoiceUserIdsData
{
    /// <summary>
    /// Gets/Sets the user ids
    /// </summary>
    [JsonPropertyName("user_ids")]
    public List<string> UserIds { get; set; }
}
