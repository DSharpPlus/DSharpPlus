
using Newtonsoft.Json;

namespace DSharpPlus.Entities;
/// <summary>
/// Gets a count of a poll answer.
/// </summary>
public sealed class DiscordPollAnswerCount
{
    /// <summary>
    /// Gets the ID of this answer.
    /// </summary>
    // Hello source code reader! I have no idea why Discord chose to do this in lieu
    // of using a dictionary. A dictionary would allow you to more easily map PollAnswer -> PollResult
    // but instead, you must loop over, check the ID, then check the ID of the current poll *answer*
    // to then build your dictionary. - Velvet
    [JsonProperty("answer_id")]
    public int AnswerId { get; internal set; }

    /// <summary>
    /// Gets a (potentially approximate) count of how many users voted for this answer.
    /// </summary>
    /// <remarks>
    /// This count isn't guaranteed to be precise unless <see cref="DiscordPollResult.IsFinalized"/> is <c>true</c>.
    /// </remarks>
    public int Count { get; internal set; }

    /// <summary>
    /// Gets whether the current user voted for this answer.
    /// </summary>
    [JsonProperty("me_voted")]
    public bool SelfVoted { get; internal set; }

    internal DiscordPollAnswerCount() { }
}
