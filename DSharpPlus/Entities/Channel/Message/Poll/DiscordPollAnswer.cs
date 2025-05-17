using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents an answer to a poll.
/// </summary>
public class DiscordPollAnswer
{
    /// <summary>
    /// Gets the ID of the answer. 
    /// </summary>
    [JsonProperty("answer_id")]
    public int AnswerId { get; internal set; }

    /// <summary>
    /// Gets the data for the answer.
    /// </summary>
    [JsonProperty("poll_media")]
    public DiscordPollMedia AnswerData { get; internal set; }
}
