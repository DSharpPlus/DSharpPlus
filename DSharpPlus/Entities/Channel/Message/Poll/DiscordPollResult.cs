
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;
public sealed class DiscordPollResult
{
    /// <summary>
    /// Gets whether the poll answers have been precisely tallied.
    /// </summary>
    [JsonProperty("is_finalized")]
    public bool IsFinalized { get; internal set; }

    /// <summary>
    /// Gets the results of the poll.
    /// </summary>
    [JsonProperty("answer_counts")]
    public IReadOnlyList<DiscordPollAnswerCount> Results { get; internal set; }
}
